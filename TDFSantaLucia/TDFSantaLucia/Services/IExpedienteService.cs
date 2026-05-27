using TDFSantaLucia.Models;

namespace TDFSantaLucia.Services
{
    public interface IExpedienteService
    {
        List<Expediente> ObtenerTodos();
        Expediente? ObtenerDetalle(int id);
        List<Expediente> ObtenerPorCliente(int clienteId);
        (bool exito, string? error) CrearExpediente(Expediente expediente);
        (bool exito, string? error) ActualizarExpediente(int id, Expediente expediente);
        (bool exito, string? error) EliminarExpediente(int id);
    }
}