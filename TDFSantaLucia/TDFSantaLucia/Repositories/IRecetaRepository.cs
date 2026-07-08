using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public interface IRecetaRepository
    {
        List<RecetaMedica> ObtenerTodas();
        List<RecetaMedica> ObtenerPorExpediente(int expedienteId);
        RecetaMedica? ObtenerPorId(int id);
        void Agregar(RecetaMedica receta);
        void Actualizar(RecetaMedica receta);
        void Eliminar(int id);
    }
}