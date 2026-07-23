using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Data;
using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public class RecetaRepository : IRecetaRepository
    {
        private readonly AppDbContext _db;

        public RecetaRepository(AppDbContext db) => _db = db;

        public List<RecetaMedica> ObtenerTodas()
            => _db.RecetasMedicas
                .Include(r => r.Producto)
                .Include(r => r.Expediente)
                    .ThenInclude(e => e.Cliente)
                        .ThenInclude(c => c.Usuario)
                .OrderByDescending(r => r.Fecha_Emision)
                .ToList();

        public List<RecetaMedica> ObtenerPorExpediente(int expedienteId)
            => _db.RecetasMedicas
                .Include(r => r.Producto)
                .Where(r => r.Expediente_Id == expedienteId)
                .OrderByDescending(r => r.Fecha_Emision)
                .ToList();

        public RecetaMedica? ObtenerPorId(int id)
            => _db.RecetasMedicas
                .Include(r => r.Producto)
                .Include(r => r.Expediente)
                    .ThenInclude(e => e.Cliente)
                        .ThenInclude(c => c.Usuario)
                .FirstOrDefault(r => r.Receta_Id == id);

        public void Agregar(RecetaMedica receta)
        {
            _db.RecetasMedicas.Add(receta);
            _db.SaveChanges();
        }

        public void Actualizar(RecetaMedica receta)
        {
            var existente = _db.RecetasMedicas.Local
                .FirstOrDefault(r => r.Receta_Id == receta.Receta_Id);
            if (existente != null)
                _db.Entry(existente).State = EntityState.Detached;

            _db.RecetasMedicas.Update(receta);
            _db.SaveChanges();
        }

        public void Eliminar(int id)
        {
            var receta = _db.RecetasMedicas.Find(id);
            if (receta != null)
            {
                _db.RecetasMedicas.Remove(receta);
                _db.SaveChanges();
            }
        }

        public List<RecetaMedica> ObtenerPorCliente(int clienteId)
             => _db.RecetasMedicas
            .Include(r => r.Producto)
            .Include(r => r.Expediente)
            .Where(r => r.Expediente!.Cliente_Id == clienteId)
            .OrderByDescending(r => r.Fecha_Emision)
            .ToList();
    }
}