using TDFSantaLucia.Data;
using TDFSantaLucia.Models;
using Microsoft.EntityFrameworkCore;

namespace TDFSantaLucia.Repositories
{
    public class InventarioRepository : IInventarioRepository
    {
        private readonly AppDbContext _db;

        public InventarioRepository(AppDbContext db)
        {
            _db = db;
        }

        public List<Inventario> ObtenerTodos()
            => _db.Inventarios
                .Include(i => i.Producto)
                    .ThenInclude(p => p.Categoria)
                .OrderByDescending(i => i.Fecha_Ingreso)
                .ToList();

        public Inventario? ObtenerPorId(int id)
            => _db.Inventarios
                .Include(i => i.Producto)
                    .ThenInclude(p => p.Categoria)
                .FirstOrDefault(i => i.Inventario_Id == id);

        public List<Inventario> ObtenerPorProducto(int productoId)
            => _db.Inventarios
                .Include(i => i.Producto)
                .Where(i => i.Producto_Id == productoId)
                .OrderByDescending(i => i.Fecha_Ingreso)
                .ToList();

        public List<Inventario> ObtenerStockBajo()
            => _db.Inventarios
                .Include(i => i.Producto)
                .Where(i => i.Estado && i.Cantidad_Disponible <= i.Cantidad_Minima)
                .OrderBy(i => i.Cantidad_Disponible)
                .ToList();

        public List<Inventario> ObtenerProximosAVencer(int diasAlerta)
        {
            var hoy = DateTime.Today;
            var limite = hoy.AddDays(diasAlerta);
            return _db.Inventarios
                .Include(i => i.Producto)
                .Where(i => i.Estado && i.Fecha_Vencimiento.Date <= limite && i.Fecha_Vencimiento.Date >= hoy)
                .OrderBy(i => i.Fecha_Vencimiento)
                .ToList();
        }

        public bool ExisteNumeroLote(string numeroLote)
            => _db.Inventarios.Any(i => i.Numero_Lote == numeroLote);

        public bool ExisteNumeroLoteEnOtro(string numeroLote, int idExcluir)
            => _db.Inventarios.Any(i => i.Numero_Lote == numeroLote && i.Inventario_Id != idExcluir);

        public void Agregar(Inventario inventario)
        {
            _db.Inventarios.Add(inventario);
            _db.SaveChanges();
        }

        public void Actualizar(Inventario inventario)
        {
            var existing = _db.Inventarios.Local
                .FirstOrDefault(i => i.Inventario_Id == inventario.Inventario_Id);

            if (existing != null)
                _db.Entry(existing).State = EntityState.Detached;

            _db.Inventarios.Update(inventario);
            _db.SaveChanges();
        }

        public void Eliminar(int id)
        {
            var inventario = _db.Inventarios.Find(id);
            if (inventario != null)
            {
                _db.Inventarios.Remove(inventario);
                _db.SaveChanges();
            }
        }
    }
}