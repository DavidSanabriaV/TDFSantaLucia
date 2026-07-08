using TDFSantaLucia.Models;

namespace TDFSantaLucia.Services
{
    public interface ITratamientoService
    {
        List<Tratamiento> ObtenerPorCliente(int clienteId);
        Tratamiento? ObtenerPorId(int id);
        List<RecordatorioTratamiento> ObtenerRecordatoriosActivos(int clienteId);
        (bool exito, string? error) Crear(TratamientoViewModel model, int clienteId);
        (bool exito, string? error) Actualizar(int id, TratamientoViewModel model);
        (bool exito, string? error) Eliminar(int id, int clienteId);
        (bool exito, string? error) ToggleAlertas(int id, int clienteId);
    }
}