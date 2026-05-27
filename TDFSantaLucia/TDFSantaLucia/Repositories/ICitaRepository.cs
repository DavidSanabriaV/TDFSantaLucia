using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public interface ICitaRepository
    {
        List<Cita> ObtenerTodas();
        List<Cita> ObtenerPorCliente(int clienteId);
        Cita? ObtenerPorId(int id);
        void Agregar(Cita cita);
        void Actualizar(Cita cita);
        void Eliminar(int id);
        bool EmpleadoTieneCitaEnHorario(int empleadoId, DateTime fecha, int? excluirCitaId = null);
    }
}