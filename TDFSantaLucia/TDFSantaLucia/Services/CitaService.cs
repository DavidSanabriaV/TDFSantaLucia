using TDFSantaLucia.Models;
using TDFSantaLucia.Repositories;

namespace TDFSantaLucia.Services
{
    public class CitaService : ICitaService
    {
        private readonly ICitaRepository _citaRepo;
        private readonly IClienteRepository _clienteRepo;
        private readonly IEmpleadoRepository _empleadoRepo;

        public CitaService(
            ICitaRepository citaRepo,
            IClienteRepository clienteRepo,
            IEmpleadoRepository empleadoRepo)
        {
            _citaRepo = citaRepo;
            _clienteRepo = clienteRepo;
            _empleadoRepo = empleadoRepo;
        }

        public List<Cita> ObtenerTodas()
            => _citaRepo.ObtenerTodas();

        public List<Cita> ObtenerPorCliente(int clienteId)
            => _citaRepo.ObtenerPorCliente(clienteId);

        public Cliente? ObtenerClientePorUsuarioId(string usuarioId)
            => _clienteRepo.ObtenerTodos()
                .FirstOrDefault(c => c.Usuario_ID == usuarioId);

        public Cita? ObtenerPorId(int id)
            => _citaRepo.ObtenerPorId(id);

        public CitaViewModel ObtenerViewModel(int? citaId = null)
        {
            var model = new CitaViewModel
            {
                Clientes = _clienteRepo.ObtenerTodos(),
                Empleados = _empleadoRepo.ObtenerTodos()
            };

            if (citaId.HasValue)
            {
                var cita = _citaRepo.ObtenerPorId(citaId.Value);
                if (cita != null)
                {
                    model.Cita_Id = cita.Cita_Id;
                    model.Servicio = cita.Servicio;
                    model.Fecha = cita.Fecha;
                    model.Observaciones = cita.Observaciones;
                    model.Estado = cita.Estado;
                    model.Cliente_Id = cita.Cliente_Id;
                    model.Empleado_Id = cita.Empleado_Id;
                }
            }

            return model;
        }

        public (bool success, string? error) AgendarCita(CitaViewModel model)
        {
            if (model.Fecha < DateTime.Now)
                return (false, "La fecha no puede ser en el pasado");

            if (model.Cliente_Id <= 0)
                return (false, "Cliente no válido");

            var cita = new Cita
            {
                Servicio = model.Servicio,
                Fecha = model.Fecha,
                Observaciones = model.Observaciones,
                Cliente_Id = model.Cliente_Id,
                Estado = "En Proceso"
            };

            _citaRepo.Agregar(cita);
            return (true, null);
        }

        public (bool success, string? error) AsignarEmpleado(int citaId, int empleadoId)
        {
            var cita = _citaRepo.ObtenerPorId(citaId);
            if (cita == null)
                return (false, "Cita no encontrada");

            if (_citaRepo.EmpleadoTieneCitaEnHorario(empleadoId, cita.Fecha, citaId))
                return (false, "El empleado ya tiene una cita en ese horario");

            cita.Empleado_Id = empleadoId;
            _citaRepo.Actualizar(cita);
            return (true, null);
        }

        public (bool success, string? error) CambiarEstado(int citaId, string estado)
        {
            var cita = _citaRepo.ObtenerPorId(citaId);
            if (cita == null)
                return (false, "Cita no encontrada");

            cita.Estado = estado;
            _citaRepo.Actualizar(cita);
            return (true, null);
        }

        public bool EliminarCita(int id)
        {
            var cita = _citaRepo.ObtenerPorId(id);
            if (cita == null) return false;

            _citaRepo.Eliminar(id);
            return true;
        }
    }
}