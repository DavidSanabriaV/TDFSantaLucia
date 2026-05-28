using TDFSantaLucia.Data;
using TDFSantaLucia.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace TDFSantaLucia.Repositories
{
    public class ProductoRepository : IProductoRepository
    {
        private readonly AppDbContext _db;
        public ProductoRepository(AppDbContext db) => _db = db;

        public List<Producto> ObtenerTodos()
            => _db.Productos.Include(p => p.Categoria).AsNoTracking().ToList();

        public Producto? ObtenerPorId(int id)
    => _db.Productos
        .Include(p => p.Categoria)
        .FirstOrDefault(p => p.Producto_Id == id);


        public void Agregar(Producto entidad)
        {
            _db.Productos.Add(entidad);
            _db.SaveChanges();
        }

        public void Actualizar(Producto entidad)
        {
            var existente = _db.Productos.Local
                .FirstOrDefault(p => p.Producto_Id == entidad.Producto_Id);

            if (existente != null)
                _db.Entry(existente).State = EntityState.Detached;

            _db.Productos.Update(entidad);
            _db.SaveChanges();
        }

        public void Eliminar(int id)
        {
            var e = _db.Productos.Find(id);
            if (e == null) return;
            _db.Productos.Remove(e);
            _db.SaveChanges();
        }

        public bool ExisteNombre(string nombre)
            => _db.Productos.Any(p => p.Nombre == nombre);

        public bool ExisteNombreEnOtra(string nombre, int id)
            => _db.Productos.Any(p => p.Nombre == nombre && p.Producto_Id != id);
    }
}