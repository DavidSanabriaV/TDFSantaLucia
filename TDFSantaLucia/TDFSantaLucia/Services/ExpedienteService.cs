using TDFSantaLucia.Models;
using TDFSantaLucia.Repositories;

namespace TDFSantaLucia.Services
{
    public class ExpedienteService : IExpedienteService
    {
        private readonly IExpedienteRepository _repository;

        public ExpedienteService(IExpedienteRepository repository)
        {
            _repository = repository;
        }

        public List<Expediente> ObtenerTodos()
            => _repository.ObtenerTodos();

        public Expediente? ObtenerDetalle(int id)
            => _repository.ObtenerPorId(id);

        public List<Expediente> ObtenerPorCliente(int clienteId)
            => _repository.ObtenerPorCliente(clienteId);

        public (bool exito, string? error) CrearExpediente(Expediente expediente)
        {
            expediente.Descripcion = expediente.Descripcion?.Trim();
            expediente.Fecha_Creacion = DateTime.Now;
            expediente.Fecha_Actualizacion = DateTime.Now;

            _repository.Agregar(expediente);
            return (true, null);
        }

        public (bool exito, string? error) ActualizarExpediente(int id, Expediente expediente)
        {
            var existente = _repository.ObtenerPorId(id);
            if (existente == null)
                return (false, "El expediente no existe");

            existente.Descripcion = expediente.Descripcion?.Trim();
            existente.Cliente_Id = expediente.Cliente_Id;
            existente.Empleado_Id = expediente.Empleado_Id;
            existente.Fecha_Actualizacion = DateTime.Now;

            _repository.Actualizar(existente);
            return (true, null);
        }

        public (bool exito, string? error) EliminarExpediente(int id)
        {
            var expediente = _repository.ObtenerPorId(id);
            if (expediente == null)
                return (false, "El expediente no existe");

            if (expediente.RecetasMedicas.Any())
                return (false, $"No se puede eliminar porque tiene {expediente.RecetasMedicas.Count} receta(s) médica(s) asociada(s)");

            _repository.Eliminar(id);
            return (true, null);
        }
    }
}