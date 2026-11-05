using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Data;
using TDFSantaLucia.Models;
using TDFSantaLucia.Repositories;

namespace TDFSantaLucia.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly IPedidoRepository _pedidoRepo;
        private readonly IFacturaRepository _facturaRepo;
        private readonly IInventarioRepository _inventarioRepo;
        private readonly IProductoRepository _productoRepo;
        private readonly IMovimientoInventarioRepository _movimientoRepo;
        private readonly AppDbContext _db;

        private const decimal TasaIVA = 0.13m;
        private const int MesesVencimiento = 24;
        private const int ColonesPorPunto = 100;

        public PedidoService(
            IPedidoRepository pedidoRepo,
            IFacturaRepository facturaRepo,
            IInventarioRepository inventarioRepo,
            IProductoRepository productoRepo,
            IMovimientoInventarioRepository movimientoRepo,
            AppDbContext db)
        {
            _pedidoRepo = pedidoRepo;
            _facturaRepo = facturaRepo;
            _inventarioRepo = inventarioRepo;
            _productoRepo = productoRepo;
            _movimientoRepo = movimientoRepo;
            _db = db;
        }

        public List<Pedido> ObtenerTodos() => _pedidoRepo.ObtenerTodos();
        public List<Pedido> ObtenerPorCliente(int clienteId) => _pedidoRepo.ObtenerPorCliente(clienteId);
        public Pedido? ObtenerPorId(int id) => _pedidoRepo.ObtenerPorId(id);
        public void ActualizarPedido(Pedido pedido) => _pedidoRepo.Actualizar(pedido);

        // ════════════════════════════════════════════════════════════════
        //  PROCESAR PEDIDO (checkout)
        // ════════════════════════════════════════════════════════════════
        public async Task<(bool exito, string? error, Pedido? pedido)>
            ProcesarPedidoAsync(CheckoutViewModel checkout, int clienteId)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // ── Validar puntos (los "Pendiente" no cuentan) ───────────
                if (checkout.Canjear_Puntos && checkout.Puntos_A_Canjear > 0)
                {
                    var puntosActuales = await _db.MovimientosPuntos
                        .Where(m => m.Cliente_Id == clienteId
                                 && !m.Vencido
                                 && m.Tipo != "Pendiente")
                        .SumAsync(m => m.Puntos);

                    if (checkout.Puntos_A_Canjear > puntosActuales)
                    {
                        await transaction.RollbackAsync();
                        return (false,
                            $"No tienes suficientes puntos. Disponibles: {puntosActuales}.", null);
                    }
                }

                // ── Validar cupón ─────────────────────────────────────────
                if (checkout.Cupon_Id.HasValue && checkout.ClienteCupon_Id > 0)
                {
                    var cc = await _db.ClientesCupones
                        .Include(x => x.Cupon)
                        .FirstOrDefaultAsync(x => x.Cupon_Cliente_Id == checkout.ClienteCupon_Id
                                                && x.Cliente_Id == clienteId
                                                && !x.Utilizado);

                    if (cc == null || cc.Cupon == null || !cc.Cupon.Estado
                        || cc.Cupon.Fecha_Expiracion.Date < DateTime.Today)
                    {
                        checkout.Cupon_Id = null;
                        checkout.ClienteCupon_Id = 0;
                        checkout.Descuento_Cupon_Raw = "0";
                    }
                }

                // ── Stock: descuento atómico por lote ─────────────────────
                var detalles = new List<DetallePedido>();
                var movimientosPendientes = new List<(int ProductoId, int InventarioId, int Cantidad)>();

                foreach (var item in checkout.Items)
                {
                    var lotes = await _db.Inventarios
                        .AsNoTracking()
                        .Where(i => i.Producto_Id == item.Producto_Id
                                 && i.Estado && i.Cantidad_Disponible > 0)
                        .OrderBy(i => i.Fecha_Vencimiento)
                        .ToListAsync();

                    var stockTotal = lotes.Sum(l => l.Cantidad_Disponible);
                    if (stockTotal < item.Cantidad)
                    {
                        await transaction.RollbackAsync();
                        return (false,
                            $"Stock insuficiente para '{item.Nombre}'. " +
                            $"Disponible: {stockTotal}, solicitado: {item.Cantidad}", null);
                    }

                    int pendiente = item.Cantidad;
                    foreach (var lote in lotes)
                    {
                        if (pendiente <= 0) break;
                        int descontar = Math.Min(lote.Cantidad_Disponible, pendiente);
                        var loteId = lote.Inventario_Id;

                        // UPDATE ... WHERE Cantidad_Disponible >= descontar
                        // Si otro cliente se llevó el stock primero, filas = 0.
                        var filas = await _db.Inventarios
                            .Where(i => i.Inventario_Id == loteId
                                     && i.Cantidad_Disponible >= descontar)
                            .ExecuteUpdateAsync(s => s.SetProperty(
                                i => i.Cantidad_Disponible,
                                i => i.Cantidad_Disponible - descontar));

                        if (filas == 0)
                        {
                            await transaction.RollbackAsync();
                            return (false,
                                $"'{item.Nombre}' acaba de agotarse o cambió su stock. " +
                                "Revisa tu carrito e intenta de nuevo.", null);
                        }

                        pendiente -= descontar;
                        movimientosPendientes.Add((item.Producto_Id, loteId, descontar));
                    }

                    detalles.Add(new DetallePedido
                    {
                        Producto_Id = item.Producto_Id,
                        Cantidad = item.Cantidad,
                        Precio_Unitario = item.Precio,
                        Subtotal = item.Subtotal
                    });
                }
                // ── Calculos ──────────────────────────────────────────────
                var subtotal = checkout.Subtotal;
                var impuesto = checkout.Impuesto;
                var descuentoPuntos = checkout.Canjear_Puntos ? checkout.Descuento_Puntos : 0;
                var descuentoCupon = checkout.Descuento_Cupon;
                var descuentoTotal = descuentoPuntos + descuentoCupon;
                var total = checkout.Total;

                int puntosGanados = 0;
                if (!checkout.Canjear_Puntos)
                    puntosGanados = (int)Math.Floor(subtotal / ColonesPorPunto);

                var requiereReceta = checkout.Items.Any(i => i.Receta);

                var pedido = new Pedido
                {
                    Numero_Orden = _pedidoRepo.GenerarNumeroOrden(),
                    Estado = PedidoEstados.Pendiente,
                    Total = total,
                    Tipo_Entrega = checkout.Tipo_Entrega,
                    Metodo_Pago = checkout.Metodo_Pago,
                    Direccion_Entrega = checkout.Direccion_Entrega,
                    Telefono_Contacto = checkout.Telefono_Contacto,
                    Cliente_Id = clienteId,
                    Fecha_Creacion = DateTime.Now,
                    Fecha_Actualizacion = DateTime.Now,
                    DetallesPedido = detalles,
                    Requiere_Receta = requiereReceta,
                    Estado_Receta = requiereReceta ? "Pendiente" : null,
                    Receta_URL = checkout.Receta_URL,
                    Puntos_Canjeados = checkout.Canjear_Puntos ? checkout.Puntos_A_Canjear : 0,
                    Descuento_Puntos = descuentoPuntos,
                    Puntos_Ganados = puntosGanados,
                    Uso_Puntos = checkout.Canjear_Puntos,
                    Cupon_Id = checkout.Cupon_Id
                };

                _pedidoRepo.Agregar(pedido);
                await _db.SaveChangesAsync();

                // ── Movimientos de inventario (SALIDA por lote) ───────────
                foreach (var mov in movimientosPendientes)
                {
                    _movimientoRepo.Agregar(new MovimientoInventario
                    {
                        Tipo_Movimiento = "SALIDA",
                        Cantidad = mov.Cantidad,
                        Descripcion = "Venta",
                        Fecha_Movimiento = DateTime.Now,
                        Producto_Id = mov.ProductoId,
                        Inventario_Id = mov.InventarioId,
                        Pedido_Id = pedido.Pedido_Id
                    });
                }
                await _db.SaveChangesAsync();

                // ── Marcar cupón como utilizado (atómico) ─────────────────
                if (checkout.Cupon_Id.HasValue && checkout.ClienteCupon_Id > 0)
                {
                    var ahora = DateTime.Now;
                    var clienteCuponId = checkout.ClienteCupon_Id;

                    var filasCupon = await _db.ClientesCupones
                        .Where(x => x.Cupon_Cliente_Id == clienteCuponId
                                 && x.Cliente_Id == clienteId
                                 && !x.Utilizado)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(x => x.Utilizado, true)
                            .SetProperty(x => x.Fecha_Uso, (DateTime?)ahora));

                    if (filasCupon == 0)
                    {
                        await transaction.RollbackAsync();
                        return (false, "El cupón ya fue utilizado.", null);
                    }
                }

                // ── Puntos canjeados ──────────────────────────────────────
                if (checkout.Canjear_Puntos && checkout.Puntos_A_Canjear > 0)
                {
                    await _db.MovimientosPuntos.AddAsync(new MovimientoPuntos
                    {
                        Puntos = -checkout.Puntos_A_Canjear,
                        Tipo = "Canjeado",
                        Descripcion = $"Canje en pedido {pedido.Numero_Orden}",
                        Fecha = DateTime.Now,
                        Fecha_Vencimiento = DateTime.Now,
                        Vencido = false,
                        Cliente_Id = clienteId,
                        Pedido_Id = pedido.Pedido_Id
                    });
                }

                // ── Puntos ganados (quedan "Pendiente" hasta confirmar) ───
                if (puntosGanados > 0)
                {
                    await _db.MovimientosPuntos.AddAsync(new MovimientoPuntos
                    {
                        Puntos = puntosGanados,
                        Tipo = "Pendiente",
                        Descripcion = $"Puntos por pedido {pedido.Numero_Orden}",
                        Fecha = DateTime.Now,
                        Fecha_Vencimiento = DateTime.Now.AddMonths(MesesVencimiento),
                        Vencido = false,
                        Cliente_Id = clienteId,
                        Pedido_Id = pedido.Pedido_Id
                    });
                }

                await _db.SaveChangesAsync();

                // ── Factura ───────────────────────────────────────────────
                var factura = new Factura
                {
                    Numero_Factura = _facturaRepo.GenerarNumeroFactura(),
                    Subtotal = subtotal,
                    Descuento = descuentoTotal,
                    Impuesto = impuesto,
                    Total = total,
                    Estado = "Emitida",
                    Fecha_Emision = DateTime.Now,
                    Cliente_Id = clienteId,
                    Pedido_Id = pedido.Pedido_Id,
                    DetallesFactura = detalles.Select(d => new DetalleFactura
                    {
                        Producto_Id = d.Producto_Id,
                        Cantidad = d.Cantidad,
                        Precio_Unitario = d.Precio_Unitario,
                        Subtotal = d.Subtotal
                    }).ToList()
                };

                _facturaRepo.Agregar(factura);
                await _db.SaveChangesAsync();

                // ── Desactivar productos sin stock ────────────────────────
                foreach (var item in checkout.Items)
                {
                    var tieneStock = await _db.Inventarios
                        .AnyAsync(i => i.Producto_Id == item.Producto_Id
                                    && i.Estado && i.Cantidad_Disponible > 0);

                    if (!tieneStock)
                    {
                        var producto = await _db.Productos
                            .FirstOrDefaultAsync(p => p.Producto_Id == item.Producto_Id);
                        if (producto != null && producto.Estado)
                        {
                            producto.Estado = false;
                            _db.Productos.Update(producto);
                        }
                    }
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, null, pedido);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error al procesar el pedido: {ex.Message}", null);
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  CAMBIAR ESTADO
        // ════════════════════════════════════════════════════════════════
        public async Task<(bool exito, string? error)>
            CambiarEstadoAsync(int pedidoId, string nuevoEstado)
        {
            var pedido = _pedidoRepo.ObtenerPorId(pedidoId);
            if (pedido == null) return (false, "Pedido no encontrado.");

            var estadosValidos = new[]
            {
                PedidoEstados.Pendiente, PedidoEstados.Aceptado,
                PedidoEstados.Rechazado, PedidoEstados.EnProceso,
                PedidoEstados.Listo,     PedidoEstados.EnCamino,
                PedidoEstados.Entregado, PedidoEstados.Cancelado
            };

            if (!estadosValidos.Contains(nuevoEstado))
                return (false, "Estado no válido.");

            var yaAnulado = pedido.Estado == PedidoEstados.Cancelado
                         || pedido.Estado == PedidoEstados.Rechazado;

            if (yaAnulado)
                return (false, "El pedido ya fue cancelado o rechazado y no se puede modificar.");

            var esAnulacion = nuevoEstado == PedidoEstados.Rechazado
                           || nuevoEstado == PedidoEstados.Cancelado;

            var estadosConfirmados = new[]
            {
                PedidoEstados.Aceptado, PedidoEstados.EnProceso, PedidoEstados.Listo,
                PedidoEstados.EnCamino, PedidoEstados.Entregado
            };

            var ahora = DateTime.Now;

            await using var tx = await _db.Database.BeginTransactionAsync();

            try
            {
                // Cambio de estado atómico: solo procede si el pedido NO fue anulado
                // por otra persona mientras tanto (evita devolver el stock dos veces).
                var filas = await _db.Pedidos
                    .Where(p => p.Pedido_Id == pedidoId
                             && p.Estado != PedidoEstados.Cancelado
                             && p.Estado != PedidoEstados.Rechazado)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(p => p.Estado, nuevoEstado)
                        .SetProperty(p => p.Fecha_Actualizacion, ahora));

                if (filas == 0)
                {
                    await tx.RollbackAsync();
                    return (false, "El pedido ya fue cancelado o rechazado.");
                }

                // ── Confirmado: los puntos pendientes pasan a ganados ─────
                if (estadosConfirmados.Contains(nuevoEstado))
                {
                    var vencimiento = ahora.AddMonths(MesesVencimiento);

                    await _db.MovimientosPuntos
                        .Where(m => m.Pedido_Id == pedidoId && m.Tipo == "Pendiente")
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(m => m.Tipo, "Ganado")
                            .SetProperty(m => m.Fecha, ahora)
                            .SetProperty(m => m.Fecha_Vencimiento, vencimiento));
                }

                // ── Rechazado / Cancelado: devolver stock, puntos, cupón ──
                if (esAnulacion)
                {
                    // 1) Stock: se devuelve a los mismos lotes de donde salió
                    var salidas = await _db.MovimientosInventario
                        .AsNoTracking()
                        .Where(m => m.Pedido_Id == pedidoId && m.Tipo_Movimiento == "SALIDA")
                        .ToListAsync();

                    if (salidas.Any())
                    {
                        foreach (var salida in salidas)
                        {
                            if (salida.Inventario_Id == null) continue;

                            var loteId = salida.Inventario_Id.Value;
                            var cantidad = salida.Cantidad;

                            await _db.Inventarios
                                .Where(i => i.Inventario_Id == loteId)
                                .ExecuteUpdateAsync(s => s.SetProperty(
                                    i => i.Cantidad_Disponible,
                                    i => i.Cantidad_Disponible + cantidad));

                            _movimientoRepo.Agregar(new MovimientoInventario
                            {
                                Tipo_Movimiento = "ENTRADA",
                                Cantidad = cantidad,
                                Descripcion = $"Devolución por pedido {nuevoEstado.ToLower()}",
                                Fecha_Movimiento = DateTime.Now,
                                Producto_Id = salida.Producto_Id,
                                Inventario_Id = loteId,
                                Pedido_Id = pedidoId
                            });
                        }
                    }
                    else
                    {
                        // Pedidos antiguos sin movimientos de salida: se devuelve
                        // al lote con vencimiento más cercano (comportamiento anterior).
                        foreach (var detalle in pedido.DetallesPedido)
                        {
                            var lote = await _db.Inventarios
                                .AsNoTracking()
                                .Where(i => i.Producto_Id == detalle.Producto_Id && i.Estado)
                                .OrderBy(i => i.Fecha_Vencimiento)
                                .FirstOrDefaultAsync();

                            if (lote == null) continue;

                            var loteId = lote.Inventario_Id;
                            var cantidad = detalle.Cantidad;

                            await _db.Inventarios
                                .Where(i => i.Inventario_Id == loteId)
                                .ExecuteUpdateAsync(s => s.SetProperty(
                                    i => i.Cantidad_Disponible,
                                    i => i.Cantidad_Disponible + cantidad));

                            _movimientoRepo.Agregar(new MovimientoInventario
                            {
                                Tipo_Movimiento = "ENTRADA",
                                Cantidad = cantidad,
                                Descripcion = $"Devolución por pedido {nuevoEstado.ToLower()}",
                                Fecha_Movimiento = DateTime.Now,
                                Producto_Id = detalle.Producto_Id,
                                Inventario_Id = loteId,
                                Pedido_Id = pedidoId
                            });
                        }
                    }

                    // 2) Reactivar los productos que se habían desactivado por falta de stock
                    var productosIds = pedido.DetallesPedido
                        .Select(d => d.Producto_Id)
                        .Distinct()
                        .ToList();

                    await _db.Productos
                        .Where(p => productosIds.Contains(p.Producto_Id) && !p.Estado)
                        .ExecuteUpdateAsync(s => s.SetProperty(p => p.Estado, true));

                    // 3) Puntos ganados (pendientes o ya acreditados) se anulan
                    await _db.MovimientosPuntos
                        .Where(m => m.Pedido_Id == pedidoId
                                 && (m.Tipo == "Pendiente" || m.Tipo == "Ganado"))
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(m => m.Tipo, "Devuelto")
                            .SetProperty(m => m.Puntos, 0));

                    // 4) Puntos canjeados se devuelven al cliente
                    if (pedido.Uso_Puntos && pedido.Puntos_Canjeados > 0)
                    {
                        await _db.MovimientosPuntos.AddAsync(new MovimientoPuntos
                        {
                            Puntos = pedido.Puntos_Canjeados,
                            Tipo = "Devuelto",
                            Descripcion = $"Devolución por pedido {pedido.Numero_Orden} {nuevoEstado.ToLower()}",
                            Fecha = ahora,
                            Fecha_Vencimiento = ahora.AddMonths(MesesVencimiento),
                            Vencido = false,
                            Cliente_Id = pedido.Cliente_Id,
                            Pedido_Id = pedidoId
                        });
                    }

                    // 5) Cupón se devuelve al cliente
                    if (pedido.Cupon_Id.HasValue)
                    {
                        var cuponId = pedido.Cupon_Id.Value;
                        var clienteId = pedido.Cliente_Id;

                        await _db.ClientesCupones
                            .Where(x => x.Cupon_Id == cuponId
                                     && x.Cliente_Id == clienteId
                                     && x.Utilizado)
                            .ExecuteUpdateAsync(s => s
                                .SetProperty(x => x.Utilizado, false)
                                .SetProperty(x => x.Fecha_Uso, (DateTime?)null));
                    }

                    // 6) Factura anulada
                    await _db.Facturas
                        .Where(f => f.Pedido_Id == pedidoId)
                        .ExecuteUpdateAsync(s => s.SetProperty(f => f.Estado, "Anulada"));
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, $"Error al cambiar el estado del pedido: {ex.Message}");
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  ELIMINAR
        // ════════════════════════════════════════════════════════════════
        public (bool exito, string? error) EliminarPedido(int pedidoId)
        {
            var pedido = _pedidoRepo.ObtenerPorId(pedidoId);
            if (pedido == null) return (false, "Pedido no encontrado.");

            var estadosNoeliminables = new[]
            {
                PedidoEstados.EnProceso, PedidoEstados.Listo,
                PedidoEstados.EnCamino,  PedidoEstados.Entregado
            };

            if (estadosNoeliminables.Contains(pedido.Estado))
                return (false, "No se puede eliminar un pedido en este estado.");

            _pedidoRepo.Eliminar(pedidoId);
            return (true, null);
        }
    }
}
