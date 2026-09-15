using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Data;
using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public class EmpleadoRepository : IEmpleadoRepository
    {
        private readonly AppDbContext _context;

        public EmpleadoRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Empleado> ObtenerTodos()
        {
            return _context.Empleados
                .AsNoTracking()
                .Include(e => e.Usuario)
                .ToList();
        }

        public Empleado? ObtenerPorId(int id)
        {
            return _context.Empleados
                .Include(e => e.Usuario)
                .Include(e => e.Citas)
                .Include(e => e.Horarios)
                .Include(e => e.Expedientes)
                .Include(e => e.ContactosEmergencia)
                .Include(e => e.Alergias)
                .Include(e => e.Enfermedades)
                .FirstOrDefault(e => e.Empleado_Id == id);
        }

        public void Agregar(Empleado empleado)
        {
            _context.Empleados.Add(empleado);
            _context.SaveChanges();
        }

        public void Actualizar(Empleado empleado)
        {
            var existente = _context.Empleados
                .Include(e => e.ContactosEmergencia)
                .Include(e => e.Alergias)
                .Include(e => e.Enfermedades)
                .FirstOrDefault(e => e.Empleado_Id == empleado.Empleado_Id);

            if (existente == null) return;

            existente.Cedula = empleado.Cedula;
            existente.Telefono = empleado.Telefono;
            existente.Direccion_Exacta = empleado.Direccion_Exacta;
            existente.Puesto = empleado.Puesto;
            existente.SalarioBruto = empleado.SalarioBruto;
            existente.SalarioNeto = empleado.SalarioNeto;
            existente.Estado = empleado.Estado;

            _context.ContactosEmergencia.RemoveRange(existente.ContactosEmergencia);
            _context.Alergias.RemoveRange(existente.Alergias);
            _context.Enfermedades.RemoveRange(existente.Enfermedades);

            existente.ContactosEmergencia = empleado.ContactosEmergencia;
            existente.Alergias = empleado.Alergias;
            existente.Enfermedades = empleado.Enfermedades;

            _context.SaveChanges();
        }

        public void Desactivar(int id)
        {
            var empleado = _context.Empleados.Find(id);
            if (empleado == null) return;

            empleado.Estado = false;
            _context.SaveChanges();
        }

        public void Activar(int id)
        {
            var empleado = _context.Empleados.Find(id);
            if (empleado == null) return;

            empleado.Estado = true;
            _context.SaveChanges();
        }
    }
}