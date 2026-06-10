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
            // Usar transacción para evitar condiciones de carrera
            await using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var detalles = new List<DetallePedido>();

                foreach (var item in checkout.Items)
                {
                    // Bloquear fila de inventario para evitar
                    // que dos usuarios compren el mismo stock
                    var lotes = await _db.Inventarios
                        .Where(i => i.Producto_Id == item.Producto_Id
                                 && i.Estado
                                 && i.Cantidad_Disponible > 0)
                        .OrderBy(i => i.Fecha_Vencimiento)
                        .ToListAsync();

                    var stockTotal = lotes.Sum(l => l.Cantidad_Disponible);

                    if (stockTotal < item.Cantidad)
                        return (false,
                            $"Stock insuficiente para '{item.Nombre}'. " +
                            $"Disponible: {stockTotal}, solicitado: {item.Cantidad}",
                            null);

                    // Descontar stock del lote más próximo a vencer (FIFO)
                    int pendiente = item.Cantidad;
                    foreach (var lote in lotes)
                    {
                        if (pendiente <= 0) break;
                        int descontar = Math.Min(lote.Cantidad_Disponible, pendiente);
                        lote.Cantidad_Disponible -= descontar;
                        pendiente -= descontar;
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

                // Para pedidos a domicilio el estado
                // empieza en Pendiente hasta que envíen el comprobante
                var estadoInicial = checkout.Tipo_Entrega == "Tienda"
                    ? PedidoEstados.Aceptado
                    : PedidoEstados.Pendiente;

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

                _pedidoRepo.Agregar(pedido);

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

                // Actualizar estado de productos sin stock
                foreach (var item in checkout.Items)
                {
                    var tieneStock = await _db.Inventarios
                        .AnyAsync(i => i.Producto_Id == item.Producto_Id
                                    && i.Estado
                                    && i.Cantidad_Disponible > 0);

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

            // Si se rechaza devolver el stock
            if (nuevoEstado == PedidoEstados.Rechazado ||
                nuevoEstado == PedidoEstados.Cancelado)
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

                        var producto = await _db.Productos
                            .FirstOrDefaultAsync(p => p.Producto_Id == detalle.Producto_Id);
                        if (producto != null && !producto.Estado)
                        {
                            producto.Estado = true;
                            _db.Productos.Update(producto);
                        }
                    }
                }

                await _db.SaveChangesAsync();
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
                return (false, "No se puede eliminar un pedido en este estado.");

            _pedidoRepo.Eliminar(pedidoId);
            return (true, null);
        }
    }
}