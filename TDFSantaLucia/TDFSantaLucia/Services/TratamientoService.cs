using TDFSantaLucia.Data;
using TDFSantaLucia.Models;
using TDFSantaLucia.Repositories;

namespace TDFSantaLucia.Services
{
    public class TratamientoService : ITratamientoService
    {
        private readonly ITratamientoRepository _repo;

        public TratamientoService(ITratamientoRepository repo)
        {
            _repo = repo;
        }

        public List<Tratamiento> ObtenerPorCliente(int clienteId)
            => _repo.ObtenerPorCliente(clienteId);

        public Tratamiento? ObtenerPorId(int id)
            => _repo.ObtenerPorId(id);

        public List<RecordatorioTratamiento> ObtenerRecordatoriosActivos(int clienteId)
            => _repo.ObtenerRecordatoriosActivos(clienteId);

        public (bool exito, string? error) Crear(
            TratamientoViewModel model, int clienteId)
        {
            if (model.Fecha_Fin < model.Fecha_Inicio)
                return (false,
                    "La fecha de fin no puede ser anterior a la de inicio.");

            if (!model.Horarios.Any())
                return (false,
                    "Debe agregar al menos un horario de recordatorio.");

            var horariosLimpios = model.Horarios
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .ToList();

            if (horariosLimpios.Distinct().Count() != horariosLimpios.Count)
                return (false,
                    "No puede agregar dos recordatorios a la misma hora.");

            var tratamiento = new Tratamiento
            {
                Nombre_Medicamento = model.Nombre_Medicamento.Trim(),
                Dosis = model.Dosis?.Trim(),
                Duracion = model.Duracion?.Trim(),
                Fecha_Inicio = model.Fecha_Inicio,
                Fecha_Fin = model.Fecha_Fin,
                Estado = true,
                Alertas_Activas = model.Alertas_Activas,
                Cliente_Id = clienteId,
                Recordatorios = horariosLimpios
                    .Select(h => new RecordatorioTratamiento
                    {
                        Hora = TimeSpan.Parse(h),
                        Alerta_Activa = model.Alertas_Activas,
                        Confirmacion = false
                    }).ToList()
            };

            _repo.Agregar(tratamiento);
            return (true, null);
        }

        public (bool exito, string? error) Actualizar(
            int id, TratamientoViewModel model)
        {
            var existente = _repo.ObtenerPorId(id);
            if (existente == null)
                return (false, "El tratamiento no existe.");

            if (model.Fecha_Fin < model.Fecha_Inicio)
                return (false,
                    "La fecha de fin no puede ser anterior a la de inicio.");

            if (!model.Horarios.Any())
                return (false,
                    "Debe agregar al menos un horario de recordatorio.");

            var horariosLimpios = model.Horarios
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .ToList();

            if (horariosLimpios.Distinct().Count() != horariosLimpios.Count)
                return (false,
                    "No puede agregar dos recordatorios a la misma hora.");

            existente.Nombre_Medicamento = model.Nombre_Medicamento.Trim();
            existente.Dosis = model.Dosis?.Trim();
            existente.Duracion = model.Duracion?.Trim();
            existente.Fecha_Inicio = model.Fecha_Inicio;
            existente.Fecha_Fin = model.Fecha_Fin;
            existente.Alertas_Activas = model.Alertas_Activas;

            var nuevosHorarios = horariosLimpios
                .Select(h => TimeSpan.Parse(h))
                .ToList();

            _repo.Actualizar(existente, nuevosHorarios);
            return (true, null);
        }

        public (bool exito, string? error) Eliminar(int id, int clienteId)
        {
            var tratamiento = _repo.ObtenerPorId(id);
            if (tratamiento == null)
                return (false, "El tratamiento no existe.");

            if (tratamiento.Cliente_Id != clienteId)
                return (false, "No tienes permiso para eliminar este tratamiento.");

            _repo.Eliminar(id);
            return (true, null);
        }

        public (bool exito, string? error) ToggleAlertas(int id, int clienteId)
        {
            var tratamiento = _repo.ObtenerPorId(id);
            if (tratamiento == null)
                return (false, "El tratamiento no existe.");

            if (tratamiento.Cliente_Id != clienteId)
                return (false, "No tienes permiso.");

            tratamiento.Alertas_Activas = !tratamiento.Alertas_Activas;
            foreach (var r in tratamiento.Recordatorios)
                r.Alerta_Activa = tratamiento.Alertas_Activas;

            _repo.Actualizar(tratamiento, tratamiento.Recordatorios.Select(r => r.Hora).ToList());
            return (true, null);
        }
    }
}