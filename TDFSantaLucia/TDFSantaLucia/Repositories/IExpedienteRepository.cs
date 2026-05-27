using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public interface IExpedienteRepository
    {
        List<Expediente> ObtenerTodos();
        Expediente? ObtenerPorId(int id);
        List<Expediente> ObtenerPorCliente(int clienteId);
        bool ExisteExpediente(int clienteId, int empleadoId);
        void Agregar(Expediente expediente);
        void Actualizar(Expediente expediente);
        void Eliminar(int id);
    }
}