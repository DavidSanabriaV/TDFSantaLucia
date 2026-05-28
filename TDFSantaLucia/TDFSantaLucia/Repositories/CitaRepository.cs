using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Data;
using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public class CitaRepository : ICitaRepository
    {
        private readonly AppDbContext _context;

        public CitaRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Cita> ObtenerTodas()
            => _context.Citas
                .Include(c => c.Cliente)
                    .ThenInclude(cl => cl.Usuario)
                .Include(c => c.Empleado)
                    .ThenInclude(e => e.Usuario)
                .OrderByDescending(c => c.Fecha)
                .ToList();

        public List<Cita> ObtenerPorCliente(int clienteId)
            => _context.Citas
                .Include(c => c.Cliente)
                    .ThenInclude(cl => cl.Usuario)
                .Include(c => c.Empleado)
                    .ThenInclude(e => e.Usuario)
                .Where(c => c.Cliente_Id == clienteId)
                .OrderByDescending(c => c.Fecha)
                .ToList();

        public Cita? ObtenerPorId(int id)
            => _context.Citas
                .Include(c => c.Cliente)
                    .ThenInclude(cl => cl.Usuario)
                .Include(c => c.Empleado)
                    .ThenInclude(e => e.Usuario)
                .FirstOrDefault(c => c.Cita_Id == id);

        public void Agregar(Cita cita)
        {
            _context.Citas.Add(cita);
            _context.SaveChanges();
        }

        public void Actualizar(Cita cita)
        {
            _context.Citas.Update(cita);
            _context.SaveChanges();
        }

        public void Eliminar(int id)
        {
            var cita = _context.Citas.Find(id);
            if (cita != null)
            {
                _context.Citas.Remove(cita);
                _context.SaveChanges();
            }
        }

        public bool EmpleadoTieneCitaEnHorario(int empleadoId, DateTime fecha, int? excluirCitaId = null)
        {
            var fechaMin = fecha.AddMinutes(-30);
            var fechaMax = fecha.AddMinutes(30);

            return _context.Citas.Any(c =>
                c.Empleado_Id == empleadoId &&
                (excluirCitaId == null || c.Cita_Id != excluirCitaId) &&
                c.Estado != "Cancelada" &&
                c.Fecha >= fechaMin &&
                c.Fecha <= fechaMax
            );
        }
    }
}