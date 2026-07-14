using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Data;
using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public class MovimientoInventarioRepository : IMovimientoInventarioRepository
    {
        private readonly AppDbContext _db;

        public MovimientoInventarioRepository(AppDbContext db) => _db = db;

        public List<MovimientoInventario> ObtenerPorProducto(
            int productoId, DateTime? desde, DateTime? hasta, string? tipoFiltro)
        {
            var query = _db.MovimientosInventario
                .Where(m => m.Producto_Id == productoId)
                .AsQueryable();

            if (desde.HasValue)
                query = query.Where(m => m.Fecha_Movimiento.Date >= desde.Value.Date);

            if (hasta.HasValue)
                query = query.Where(m => m.Fecha_Movimiento.Date <= hasta.Value.Date);

            if (!string.IsNullOrEmpty(tipoFiltro) && tipoFiltro != "TODOS")
                query = query.Where(m => m.Tipo_Movimiento == tipoFiltro);

            return query.OrderByDescending(m => m.Fecha_Movimiento).ToList();
        }

        public List<MovimientoInventario> ObtenerTodos(
            DateTime? desde, DateTime? hasta, string? tipoFiltro)
        {
            var query = _db.MovimientosInventario
                .Include(m => m.Producto)
                .AsQueryable();

            if (desde.HasValue)
                query = query.Where(m => m.Fecha_Movimiento.Date >= desde.Value.Date);

            if (hasta.HasValue)
                query = query.Where(m => m.Fecha_Movimiento.Date <= hasta.Value.Date);

            if (!string.IsNullOrEmpty(tipoFiltro) && tipoFiltro != "TODOS")
                query = query.Where(m => m.Tipo_Movimiento == tipoFiltro);

            return query.OrderByDescending(m => m.Fecha_Movimiento).ToList();
        }

        public void Agregar(MovimientoInventario movimiento)
        {
            _db.MovimientosInventario.Add(movimiento);
        }

        public void GuardarCambios()
        {
            _db.SaveChanges();
        }
    }
}