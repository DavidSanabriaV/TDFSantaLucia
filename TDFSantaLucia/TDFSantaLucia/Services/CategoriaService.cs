using TDFSantaLucia.Models;
using TDFSantaLucia.Repositories;

namespace TDFSantaLucia.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _repository;

        public CategoriaService(ICategoriaRepository repository)
        {
            _repository = repository;
        }

        public List<Categoria> ObtenerTodos()
            => _repository.ObtenerTodos();

        public Categoria? ObtenerDetalle(int id)
            => _repository.ObtenerPorId(id);

        public (bool exito, string? error) CrearCategoria(Categoria categoria)
        {
            if (_repository.ExisteNombre(categoria.Nombre.Trim()))
                return (false, "Ya existe una categoria con ese nombre");

            categoria.Nombre = categoria.Nombre.Trim();
            categoria.Descripcion = categoria.Descripcion?.Trim();

            _repository.Agregar(categoria);
            return (true, null);
        }

        public (bool exito, string? error) ActualizarCategoria(int id, Categoria categoria)
        {
            var existente = _repository.ObtenerPorId(id);
            if (existente == null)
                return (false, "La categoria no existe");

            if (_repository.ExisteNombreEnOtra(categoria.Nombre.Trim(), id))
                return (false, "Ya existe otra categoria con ese nombre");

            existente.Nombre = categoria.Nombre.Trim();
            existente.Descripcion = categoria.Descripcion?.Trim();
            existente.Estado = categoria.Estado;

            _repository.Actualizar(existente);
            return (true, null);
        }

        public (bool exito, string? error) EliminarCategoria(int id)
        {
            var categoria = _repository.ObtenerPorId(id);
            if (categoria == null)
                return (false, "La categoria no existe");

            if (categoria.Productos.Any())
                return (false, $"No se puede eliminar porque tiene {categoria.Productos.Count} producto(s) asociado(s)");

            _repository.Eliminar(id);
            return (true, null);
        }
    }
}