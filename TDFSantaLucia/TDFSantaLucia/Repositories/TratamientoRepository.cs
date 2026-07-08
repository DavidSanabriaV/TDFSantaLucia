using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Data;
using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public class TratamientoRepository : ITratamientoRepository
    {
        private readonly AppDbContext _db;

        public TratamientoRepository(AppDbContext db) => _db = db;

        public List<Tratamiento> ObtenerPorCliente(int clienteId)
            => _db.Tratamientos
                .Include(t => t.Recordatorios)
                .Where(t => t.Cliente_Id == clienteId)
                .OrderByDescending(t => t.Fecha_Inicio)
                .ToList();

        public Tratamiento? ObtenerPorId(int id)
            => _db.Tratamientos
                .Include(t => t.Recordatorios)
                .Include(t => t.Cliente).ThenInclude(c => c.Usuario)
                .FirstOrDefault(t => t.Tratamiento_Id == id);

        public List<RecordatorioTratamiento> ObtenerRecordatoriosActivos(int clienteId)
            => _db.RecordatoriosTratamiento
                .Include(r => r.Tratamiento)
                .Where(r => r.Tratamiento != null
                         && r.Tratamiento.Cliente_Id == clienteId
                         && r.Tratamiento.Estado
                         && r.Tratamiento.Fecha_Fin >= DateTime.Today
                         && r.Alerta_Activa)
                .ToList();

        public void Agregar(Tratamiento tratamiento)
        {
            _db.Tratamientos.Add(tratamiento);
            _db.SaveChanges();
        }

        public void Actualizar(Tratamiento tratamiento, List<TimeSpan> horarios)
        {
        
            if (!_db.Entry(tratamiento).Collection(t => t.Recordatorios).IsLoaded)
                _db.Entry(tratamiento).Collection(t => t.Recordatorios).Load();

            _db.RecordatoriosTratamiento.RemoveRange(tratamiento.Recordatorios);
            tratamiento.Recordatorios.Clear();

            foreach (var hora in horarios)
            {
                tratamiento.Recordatorios.Add(new RecordatorioTratamiento
                {
                    Hora = hora,
                    Alerta_Activa = tratamiento.Alertas_Activas,
                    Confirmacion = false
                });
            }

            _db.SaveChanges();
        }

        public void Eliminar(int id)
        {
            var tratamiento = _db.Tratamientos
                .Include(t => t.Recordatorios)
                .FirstOrDefault(t => t.Tratamiento_Id == id);

            if (tratamiento != null)
            {
                _db.RecordatoriosTratamiento
                    .RemoveRange(tratamiento.Recordatorios);
                _db.Tratamientos.Remove(tratamiento);
                _db.SaveChanges();
            }
        }
    }
}