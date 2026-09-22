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

        public (bool exito, string? error) CambiarEstado(int id, bool nuevoEstado)
        {
            var categoria = _repository.ObtenerPorId(id);
            if (categoria == null)
                return (false, "La categoria no existe");

            if (categoria.Estado == nuevoEstado)
                return (false, nuevoEstado
                    ? "La categoria ya se encuentra activa"
                    : "La categoria ya se encuentra inactiva");

            if (!nuevoEstado && categoria.Productos.Any(p => p.Estado))
            {
                var totalActivos = categoria.Productos.Count(p => p.Estado);
                return (false, $"No se puede desactivar porque tiene {totalActivos} producto(s) activo(s) asociado(s)");
            }

            _repository.CambiarEstado(id, nuevoEstado);
            return (true, null);
        }
    }
}