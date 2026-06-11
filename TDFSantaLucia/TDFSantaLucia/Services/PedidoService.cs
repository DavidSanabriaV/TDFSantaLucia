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
        private readonly AppDbContext _db;

        private const decimal TasaIVA = 0.13m;

        public PedidoService(
            IPedidoRepository pedidoRepo,
            IFacturaRepository facturaRepo,
            IInventarioRepository inventarioRepo,
            IProductoRepository productoRepo,
            AppDbContext db)
        {
            _pedidoRepo = pedidoRepo;
            _facturaRepo = facturaRepo;
            _inventarioRepo = inventarioRepo;
            _productoRepo = productoRepo;
            _db = db;
        }

        public List<Pedido> ObtenerTodos()
            => _pedidoRepo.ObtenerTodos();

        public List<Pedido> ObtenerPorCliente(int clienteId)
            => _pedidoRepo.ObtenerPorCliente(clienteId);

        public Pedido? ObtenerPorId(int id)
            => _pedidoRepo.ObtenerPorId(id);

        public async Task<(bool exito, string? error, Pedido? pedido)>
            ProcesarPedidoAsync(CheckoutViewModel checkout, int clienteId)
        {
            await using var transaction =
                await _db.Database.BeginTransactionAsync();

            try
            {
                var detalles = new List<DetallePedido>();

                // Validar stock y descontar
                foreach (var item in checkout.Items)
                {
                    var lotes = await _db.Inventarios
                        .Where(i => i.Producto_Id == item.Producto_Id
                                 && i.Estado
                                 && i.Cantidad_Disponible > 0)
                        .OrderBy(i => i.Fecha_Vencimiento)
                        .ToListAsync();

                    var stockTotal = lotes.Sum(l => l.Cantidad_Disponible);

                    if (stockTotal < item.Cantidad)
                    {
                        await transaction.RollbackAsync();
                        return (false,
                            $"Stock insuficiente para '{item.Nombre}'. " +
                            $"Disponible: {stockTotal}, solicitado: {item.Cantidad}",
                            null);
                    }

                    // FIFO: descontar del lote más próximo a vencer
                    int pendiente = item.Cantidad;
                    foreach (var lote in lotes)
                    {
                        if (pendiente <= 0) break;
                        int descontar = Math.Min(lote.Cantidad_Disponible, pendiente);
                        lote.Cantidad_Disponible -= descontar;
                        pendiente -= descontar;
                        _db.Inventarios.Update(lote);
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
                var total = checkout.Total;

                var estadoInicial = PedidoEstados.Pendiente;

                var pedido = new Pedido
                {
                    Numero_Orden = _pedidoRepo.GenerarNumeroOrden(),
                    Estado = estadoInicial,
                    Total = total,
                    Tipo_Entrega = checkout.Tipo_Entrega,
                    Metodo_Pago = checkout.Metodo_Pago,
                    Direccion_Entrega = checkout.Direccion_Entrega,
                    Telefono_Contacto = checkout.Telefono_Contacto,
                    Cliente_Id = clienteId,
                    Fecha_Creacion = DateTime.Now,
                    Fecha_Actualizacion = DateTime.Now,
                    DetallesPedido = detalles
                };

                // Agregar pedido sin SaveChanges (lo hace el repo sin guardar)
                _pedidoRepo.Agregar(pedido);
                await _db.SaveChangesAsync();

                // Generar factura
                var factura = new Factura
                {
                    Numero_Factura = _facturaRepo.GenerarNumeroFactura(),
                    Subtotal = subtotal,
                    Descuento = 0,
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

                // Desactivar productos sin stock
                foreach (var item in checkout.Items)
                {
                    var tieneStock = await _db.Inventarios
                        .AnyAsync(i => i.Producto_Id == item.Producto_Id
                                    && i.Estado
                                    && i.Cantidad_Disponible > 0);

                    if (!tieneStock)
                    {
                        var producto = await _db.Productos
                            .FirstOrDefaultAsync(p =>
                                p.Producto_Id == item.Producto_Id);

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
                return (false,
                    $"Error al procesar el pedido: {ex.Message}", null);
            }
        }

        public async Task<(bool exito, string? error)>
            CambiarEstadoAsync(int pedidoId, string nuevoEstado)
        {
            var pedido = _pedidoRepo.ObtenerPorId(pedidoId);
            if (pedido == null)
                return (false, "Pedido no encontrado.");

            var estadosValidos = new[]
            {
                PedidoEstados.Pendiente,
                PedidoEstados.Aceptado,
                PedidoEstados.Rechazado,
                PedidoEstados.EnProceso,
                PedidoEstados.Listo,
                PedidoEstados.EnCamino,
                PedidoEstados.Entregado,
                PedidoEstados.Cancelado
            };

            if (!estadosValidos.Contains(nuevoEstado))
                return (false, "Estado no válido.");

            if (nuevoEstado == PedidoEstados.Rechazado ||
                nuevoEstado == PedidoEstados.Cancelado)
            {
                if (pedido.Estado != PedidoEstados.Cancelado &&
                    pedido.Estado != PedidoEstados.Rechazado)
                {
                    foreach (var detalle in pedido.DetallesPedido)
                    {
                        var lote = await _db.Inventarios
                            .Where(i => i.Producto_Id == detalle.Producto_Id
                                     && i.Estado)
                            .OrderBy(i => i.Fecha_Vencimiento)
                            .FirstOrDefaultAsync();

                        if (lote != null)
                        {
                            lote.Cantidad_Disponible += detalle.Cantidad;
                            _db.Inventarios.Update(lote);

                            var producto = await _db.Productos
                                .FirstOrDefaultAsync(p =>
                                    p.Producto_Id == detalle.Producto_Id);

                            if (producto != null && !producto.Estado)
                            {
                                producto.Estado = true;
                                _db.Productos.Update(producto);
                            }
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
            if (pedido == null)
                return (false, "Pedido no encontrado.");

            var estadosNoeliminables = new[]
            {
                PedidoEstados.EnProceso,
                PedidoEstados.Listo,
                PedidoEstados.EnCamino,
                PedidoEstados.Entregado
            };

            if (estadosNoeliminables.Contains(pedido.Estado))
                return (false,
                    "No se puede eliminar un pedido en este estado.");

            _pedidoRepo.Eliminar(pedidoId);
            return (true, null);
        }
    }
}