using TDFSantaLucia.Models;

namespace TDFSantaLucia.Services
{
    public interface ICitaService
    {
        List<Cita> ObtenerTodas();
        List<Cita> ObtenerPorCliente(int clienteId);
        Cita? ObtenerPorId(int id);
        CitaViewModel ObtenerViewModel(int? citaId = null);
        (bool success, string? error) AgendarCita(CitaViewModel model);
        (bool success, string? error) AsignarEmpleado(int citaId, int empleadoId);
        (bool success, string? error) CambiarEstado(int citaId, string estado);
        bool EliminarCita(int id);
    }
}