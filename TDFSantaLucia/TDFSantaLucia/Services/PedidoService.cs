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

        public async Task<(bool exito, string? error, Pedido? pedido)>
            ProcesarPedidoAsync(CheckoutViewModel checkout, int clienteId)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // ── Validar puntos ────────────────────────────────────────
                if (checkout.Canjear_Puntos && checkout.Puntos_A_Canjear > 0)
                {
                    var puntosActuales = await _db.MovimientosPuntos
                        .Where(m => m.Cliente_Id == clienteId && !m.Vencido)
                        .SumAsync(m => m.Puntos);

                    if (checkout.Puntos_A_Canjear > puntosActuales)
                        return (false,
                            $"No tienes suficientes puntos. Disponibles: {puntosActuales}.", null);
                }

                // ── Validar cupón ─────────────────────────────────────────
                // Si el cupón no es válido, limpiamos solo los campos de referencia
                // (Descuento_Cupon es calculado desde Descuento_Cupon_Raw, no se asigna)
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
                        checkout.Descuento_Cupon_Raw = "0";  // ← limpia el raw en lugar del calculado
                    }
                }

                // ── Stock ─────────────────────────────────────────────────
                var detalles = new List<DetallePedido>();
                var movimientosPendientes = new List<(int ProductoId, int InventarioId, int Cantidad)>();

                foreach (var item in checkout.Items)
                {
                    var lotes = await _db.Inventarios
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
                        lote.Cantidad_Disponible -= descontar;
                        pendiente -= descontar;
                        _db.Inventarios.Update(lote);

                        movimientosPendientes.Add((item.Producto_Id, lote.Inventario_Id, descontar));
                    }

                    detalles.Add(new DetallePedido
                    {
                        Producto_Id = item.Producto_Id,
                        Cantidad = item.Cantidad,
                        Precio_Unitario = item.Precio,
                        Subtotal = item.Subtotal
                    });
                }

                await _db.SaveChangesAsync();

                var subtotal = checkout.Subtotal;
                var impuesto = checkout.Impuesto;
                var descuentoPuntos = checkout.Canjear_Puntos ? checkout.Descuento_Puntos : 0;
                var descuentoCupon = checkout.Descuento_Cupon;   // ← lee la propiedad calculada
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

                // ── Marcar cupón como utilizado ───────────────────────────

                // ── Marcar cupón como utilizado ───────────────────────────
                if (checkout.Cupon_Id.HasValue && checkout.ClienteCupon_Id > 0)
                {
                    var cc = await _db.ClientesCupones
                        .FirstOrDefaultAsync(x => x.Cupon_Cliente_Id == checkout.ClienteCupon_Id);
                    if (cc != null)
                    {
                        cc.Utilizado = true;
                        cc.Fecha_Uso = DateTime.Now;
                        await _db.SaveChangesAsync();
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

                // ── Puntos ganados ────────────────────────────────────────
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

            // ── Al aceptar: activar puntos ────────────────────────────────
            if (nuevoEstado == PedidoEstados.Aceptado)
            {
                var movPendiente = await _db.MovimientosPuntos
                    .FirstOrDefaultAsync(m => m.Pedido_Id == pedidoId && m.Tipo == "Pendiente");
                if (movPendiente != null)
                {
                    movPendiente.Tipo = "Ganado";
                    _db.MovimientosPuntos.Update(movPendiente);
                    await _db.SaveChangesAsync();
                }
            }

            // ── Al rechazar/cancelar: devolver stock, puntos y cupón ──────
            if (nuevoEstado == PedidoEstados.Rechazado ||
                nuevoEstado == PedidoEstados.Cancelado)
            {
                if (pedido.Estado != PedidoEstados.Cancelado &&
                    pedido.Estado != PedidoEstados.Rechazado)
                {
                    foreach (var detalle in pedido.DetallesPedido)
                    {
                        var lote = await _db.Inventarios
                            .Where(i => i.Producto_Id == detalle.Producto_Id && i.Estado)
                            .OrderBy(i => i.Fecha_Vencimiento)
                            .FirstOrDefaultAsync();

                        if (lote != null)
                        {
                            lote.Cantidad_Disponible += detalle.Cantidad;
                            _db.Inventarios.Update(lote);

                            _movimientoRepo.Agregar(new MovimientoInventario
                            {
                                Tipo_Movimiento = "ENTRADA",
                                Cantidad = detalle.Cantidad,
                                Descripcion = $"Devolución por pedido {nuevoEstado.ToLower()}",
                                Fecha_Movimiento = DateTime.Now,
                                Producto_Id = detalle.Producto_Id,
                                Inventario_Id = lote.Inventario_Id,
                                Pedido_Id = pedido.Pedido_Id
                            });

                            var prod = await _db.Productos
                                .FirstOrDefaultAsync(p => p.Producto_Id == detalle.Producto_Id);
                            if (prod != null && !prod.Estado)
                            {
                                prod.Estado = true;
                                _db.Productos.Update(prod);
                            }
                        }
                    }

                    var movPuntos = await _db.MovimientosPuntos
                        .Where(m => m.Pedido_Id == pedidoId
                                 && (m.Tipo == "Pendiente" || m.Tipo == "Ganado"))
                        .ToListAsync();

                    foreach (var m in movPuntos)
                    {
                        m.Tipo = "Devuelto";
                        m.Puntos = 0;
                        _db.MovimientosPuntos.Update(m);
                    }

                    if (pedido.Uso_Puntos && pedido.Puntos_Canjeados > 0)
                    {
                        await _db.MovimientosPuntos.AddAsync(new MovimientoPuntos
                        {
                            Puntos = pedido.Puntos_Canjeados,
                            Tipo = "Devuelto",
                            Descripcion = $"Devolución por pedido {pedido.Numero_Orden} rechazado",
                            Fecha = DateTime.Now,
                            Fecha_Vencimiento = DateTime.Now.AddMonths(MesesVencimiento),
                            Vencido = false,
                            Cliente_Id = pedido.Cliente_Id,
                            Pedido_Id = pedido.Pedido_Id
                        });
                    }

                    // ── Devolver cupón ────────────────────────────────────
                    if (pedido.Cupon_Id.HasValue)
                    {
                        var cc = await _db.ClientesCupones
                            .FirstOrDefaultAsync(x => x.Cupon_Id == pedido.Cupon_Id
                                                    && x.Cliente_Id == pedido.Cliente_Id
                                                    && x.Utilizado);
                        if (cc != null)
                        {
                            cc.Utilizado = false;
                            cc.Fecha_Uso = null;
                            _db.ClientesCupones.Update(cc);
                        }
                    }

                    await _db.SaveChangesAsync();
                }
            }

            pedido.Estado = nuevoEstado;
            pedido.Fecha_Actualizacion = DateTime.Now;
            _pedidoRepo.Actualizar(pedido);

            return (true, null);
        }

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