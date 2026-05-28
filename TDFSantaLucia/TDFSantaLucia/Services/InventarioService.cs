using TDFSantaLucia.Models;
using TDFSantaLucia.Repositories;

namespace TDFSantaLucia.Services
{
    public class InventarioService : IInventarioService
    {
        private readonly IInventarioRepository _repo;
        private readonly IProductoRepository _productoRepo;

        public InventarioService(IInventarioRepository repo, IProductoRepository productoRepo)
        {
            _repo = repo;
            _productoRepo = productoRepo;
        }

        public List<Inventario> ObtenerTodos()
            => _repo.ObtenerTodos();

        public Inventario? ObtenerPorId(int id)
            => _repo.ObtenerPorId(id);

        public List<Inventario> ObtenerPorProducto(int productoId)
            => _repo.ObtenerPorProducto(productoId);

        public List<Inventario> ObtenerStockBajo()
            => _repo.ObtenerStockBajo();

        public List<Inventario> ObtenerProximosAVencer(int diasAlerta = 30)
            => _repo.ObtenerProximosAVencer(diasAlerta);

        public int ContarStockBajo()
            => _repo.ObtenerStockBajo().Count;

        public int ContarProximosAVencer(int diasAlerta = 30)
            => _repo.ObtenerProximosAVencer(diasAlerta).Count;

        public List<Producto> ObtenerProductos()
            => _productoRepo.ObtenerTodos();

        public (bool exito, string? error) Crear(Inventario inventario)
        {
            var producto = _productoRepo.ObtenerPorId(inventario.Producto_Id);
            if (producto == null)
                return (false, "El producto seleccionado no existe.");

            if (!string.IsNullOrWhiteSpace(inventario.Numero_Lote) &&
                _repo.ExisteNumeroLote(inventario.Numero_Lote.Trim()))
                return (false, "Ya existe un lote con ese numero.");

            if (inventario.Cantidad_Disponible < 0)
                return (false, "La cantidad disponible no puede ser negativa.");

            if (inventario.Cantidad_Minima < 0)
                return (false, "La cantidad minima no puede ser negativa.");

            if (inventario.Fecha_Vencimiento <= DateTime.Today)
                return (false, "La fecha de vencimiento debe ser futura.");

            if (inventario.Fecha_Ingreso > DateTime.Today)
                return (false, "La fecha de ingreso no puede ser futura.");

            inventario.Numero_Lote = inventario.Numero_Lote?.Trim();
            inventario.Proveedor = inventario.Proveedor?.Trim();
            inventario.Estado = true;

            _repo.Agregar(inventario);

            // Si el producto estaba inactivo y ahora tiene stock, activarlo
            if (!producto.Estado && inventario.Cantidad_Disponible > 0)
            {
                producto.Estado = true;
                _productoRepo.Actualizar(producto);
            }

            return (true, null);
        }

        public (bool exito, string? error) Actualizar(int id, Inventario inventario)
        {
            var existente = _repo.ObtenerPorId(id);
            if (existente == null)
                return (false, "El lote de inventario no existe.");

            var producto = _productoRepo.ObtenerPorId(inventario.Producto_Id);
            if (producto == null)
                return (false, "El producto seleccionado no existe.");

            if (!string.IsNullOrWhiteSpace(inventario.Numero_Lote) &&
                _repo.ExisteNumeroLoteEnOtro(inventario.Numero_Lote.Trim(), id))
                return (false, "Ya existe otro lote con ese número.");

            if (inventario.Cantidad_Disponible < 0)
                return (false, "La cantidad disponible no puede ser negativa.");

            if (inventario.Cantidad_Minima < 0)
                return (false, "La cantidad mínima no puede ser negativa.");

            if (inventario.Fecha_Vencimiento <= DateTime.Today)
                return (false, "La fecha de vencimiento debe ser futura.");

            existente.Producto_Id = inventario.Producto_Id;
            existente.Cantidad_Disponible = inventario.Cantidad_Disponible;
            existente.Cantidad_Minima = inventario.Cantidad_Minima;
            existente.Fecha_Vencimiento = inventario.Fecha_Vencimiento;
            existente.Fecha_Ingreso = inventario.Fecha_Ingreso;
            existente.Numero_Lote = inventario.Numero_Lote?.Trim();
            existente.Proveedor = inventario.Proveedor?.Trim();
            existente.Estado = inventario.Estado;

            _repo.Actualizar(existente);
            return (true, null);
        }

        public (bool exito, string? error) Eliminar(int id)
        {
            var inventario = _repo.ObtenerPorId(id);
            if (inventario == null)
                return (false, "El lote de inventario no existe.");

            if (inventario.Cantidad_Disponible > 0)
                return (false, $"No se puede eliminar un lote con {inventario.Cantidad_Disponible} unidades disponibles. Primero vacie el stock.");

            var productoId = inventario.Producto_Id;
            _repo.Eliminar(id);

            var lotesConStock = _repo.ObtenerPorProducto(productoId)
                .Any(i => i.Estado && i.Cantidad_Disponible > 0);

            if (!lotesConStock)
            {
                var producto = _productoRepo.ObtenerPorId(productoId);
                if (producto != null && producto.Estado)
                {
                    producto.Estado = false;
                    _productoRepo.Actualizar(producto);
                }
            }

            return (true, null);
        }
    }
}