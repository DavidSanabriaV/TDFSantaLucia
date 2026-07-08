using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public interface ITratamientoRepository
    {
        List<Tratamiento> ObtenerPorCliente(int clienteId);

        Tratamiento? ObtenerPorId(int id);

        List<RecordatorioTratamiento> ObtenerRecordatoriosActivos(int clienteId);

        void Agregar(Tratamiento tratamiento);

        void Actualizar(Tratamiento tratamiento, List<TimeSpan> horarios);

        void Eliminar(int id);
    }
}