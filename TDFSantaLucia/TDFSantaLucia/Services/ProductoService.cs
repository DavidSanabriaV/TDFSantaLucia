using TDFSantaLucia.Models;
using TDFSantaLucia.Repositories;
using System.Collections.Generic;

namespace TDFSantaLucia.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _repo;
        public ProductoService(IProductoRepository repo) => _repo = repo;

        public List<Producto> ObtenerTodos() => _repo.ObtenerTodos();
        public Producto? ObtenerPorId(int id) => _repo.ObtenerPorId(id);
        public void Crear(Producto producto) => _repo.Agregar(producto);
        public void Actualizar(Producto producto) => _repo.Actualizar(producto);
        public void Eliminar(int id) => _repo.Eliminar(id);
        public bool ExisteAsync(int id) => _repo.ObtenerPorId(id) != null;
        public bool ExisteNombre(string nombre) => _repo.ExisteNombre(nombre);
        public bool ExisteNombreEnOtra(string n, int id) => _repo.ExisteNombreEnOtra(n, id);
    }
}