using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public interface IChatbotRepository
    {
        List<ChatbotOpcion> ObtenerTodas();
        List<ChatbotOpcion> ObtenerActivas();
        ChatbotOpcion? ObtenerPorId(int id);
        void Agregar(ChatbotOpcion opcion);
        void Actualizar(ChatbotOpcion opcion);
        void Eliminar(int id);
    }
}