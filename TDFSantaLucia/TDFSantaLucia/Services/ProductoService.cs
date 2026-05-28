using TDFSantaLucia.Models;
using TDFSantaLucia.Repositories;
using System.Collections.Generic;

namespace TDFSantaLucia.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _repo;
        private readonly IInventarioRepository _inventarioRepo;
        public ProductoService(IProductoRepository repo, IInventarioRepository inventarioRepo)
        {
            _repo = repo;
            _inventarioRepo = inventarioRepo;
        }

        public List<Producto> ObtenerTodos() => _repo.ObtenerTodos();
        public Producto? ObtenerPorId(int id) => _repo.ObtenerPorId(id);
        public void Crear(Producto producto) => _repo.Agregar(producto);
        public (bool exito, string? error) Actualizar(Producto producto)
        {
            // Si quiere activar el producto validar que tenga stock
            if (producto.Estado)
            {
                var tieneStock = _inventarioRepo.ObtenerPorProducto(producto.Producto_Id)
                    .Any(i => i.Estado && i.Cantidad_Disponible > 0);

                if (!tieneStock)
                    return (false, "No se puede activar el producto porque no tiene stock disponible. " +
                                  "Primero registre un lote con cantidad mayor a 0.");
            }

            if (_repo.ExisteNombreEnOtra(producto.Nombre?.Trim() ?? "", producto.Producto_Id))
                return (false, "Ya existe otro producto con ese nombre.");

            _repo.Actualizar(producto);
            return (true, null);
        }
        public void Eliminar(int id) => _repo.Eliminar(id);
        public bool ExisteAsync(int id) => _repo.ObtenerPorId(id) != null;
        public bool ExisteNombre(string nombre) => _repo.ExisteNombre(nombre);
        public bool ExisteNombreEnOtra(string n, int id) => _repo.ExisteNombreEnOtra(n, id);
    }
}