using TDFSantaLucia.Data;
using TDFSantaLucia.Models;
using Microsoft.EntityFrameworkCore;

namespace TDFSantaLucia.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly AppDbContext _context;

        public CategoriaRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Categoria> ObtenerTodos()
            => _context.Categorias
                .Include(c => c.Productos)
                .OrderBy(c => c.Nombre)
                .ToList();

        public Categoria? ObtenerPorId(int id)
            => _context.Categorias
                .Include(c => c.Productos)
                .FirstOrDefault(c => c.Categoria_Id == id);

        public bool ExisteNombre(string nombre)
            => _context.Categorias
                .Any(c => c.Nombre.ToLower() == nombre.ToLower());

        public bool ExisteNombreEnOtra(string nombre, int idExcluir)
            => _context.Categorias
                .Any(c => c.Nombre.ToLower() == nombre.ToLower()
                       && c.Categoria_Id != idExcluir);

        public void Agregar(Categoria categoria)
        {
            _context.Categorias.Add(categoria);
            _context.SaveChanges();
        }

        public void Actualizar(Categoria categoria)
        {
            var existingEntity = _context.Categorias.Local
                .FirstOrDefault(c => c.Categoria_Id == categoria.Categoria_Id);

            if (existingEntity != null)
                _context.Entry(existingEntity).State = EntityState.Detached;

            _context.Categorias.Update(categoria);
            _context.SaveChanges();
        }

        public void Eliminar(int id)
        {
            var categoria = _context.Categorias.Find(id);
            if (categoria != null)
            {
                _context.Categorias.Remove(categoria);
                _context.SaveChanges();
            }
        }
    }
}