using TDFSantaLucia.Data;
using TDFSantaLucia.Models;
using Microsoft.EntityFrameworkCore;

namespace TDFSantaLucia.Repositories
{
    public class ExpedienteRepository : IExpedienteRepository
    {
        private readonly AppDbContext _context;

        public ExpedienteRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Expediente> ObtenerTodos()
            => _context.Expedientes
                .Include(e => e.Cliente).ThenInclude(c => c.Usuario)
                .Include(e => e.Empleado).ThenInclude(emp => emp.Usuario)
                .Include(e => e.RecetasMedicas).ThenInclude(r => r.Producto)
                .OrderByDescending(e => e.Fecha_Creacion)
                .ToList();

        public Expediente? ObtenerPorId(int id)
            => _context.Expedientes
                .Include(e => e.Cliente).ThenInclude(c => c.Usuario)
                .Include(e => e.Empleado).ThenInclude(emp => emp.Usuario)
                .Include(e => e.RecetasMedicas).ThenInclude(r => r.Producto)
                .FirstOrDefault(e => e.Expediente_Id == id);

        public List<Expediente> ObtenerPorCliente(int clienteId)
            => _context.Expedientes
                .Include(e => e.Cliente).ThenInclude(c => c.Usuario)
                .Include(e => e.Empleado).ThenInclude(emp => emp.Usuario)
                .Include(e => e.RecetasMedicas).ThenInclude(r => r.Producto)
                .Where(e => e.Cliente_Id == clienteId)
                .OrderByDescending(e => e.Fecha_Creacion)
                .ToList();

        public void Agregar(Expediente expediente)
        {
            _context.Expedientes.Add(expediente);
            _context.SaveChanges();
        }

        public void Actualizar(Expediente expediente)
        {
            var existingEntity = _context.Expedientes.Local
                .FirstOrDefault(e => e.Expediente_Id == expediente.Expediente_Id);

            if (existingEntity != null)
                _context.Entry(existingEntity).State = EntityState.Detached;

            _context.Expedientes.Update(expediente);
            _context.SaveChanges();
        }

        public void Eliminar(int id)
        {
            var expediente = _context.Expedientes.Find(id);
            if (expediente != null)
            {
                _context.Expedientes.Remove(expediente);
                _context.SaveChanges();
            }
        }

        public bool ExisteExpediente(int clienteId, int empleadoId)
        {
            throw new NotImplementedException();
        }
    }
}