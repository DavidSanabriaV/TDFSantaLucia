using TDFSantaLucia.Models;

namespace TDFSantaLucia.Services
{
    public interface IChatbotService
    {
        List<ChatbotOpcion> ObtenerTodas();
        List<ChatbotOpcion> ObtenerActivas();
        ChatbotOpcion? ObtenerPorId(int id);
        (bool exito, string? error) Crear(ChatbotOpcion opcion);
        (bool exito, string? error) Actualizar(ChatbotOpcion opcion);
        (bool exito, string? error) Eliminar(int id);

        bool ExisteTexto(string texto, int? excluirId = null);
        bool ExisteOrden(int orden, int? excluirId = null);

        string? DetectarIntent(string texto);

        Task<ChatbotRespuesta> ResponderAsync(
            int? opcionId, string? textoLibre, string? usuarioId);
    }

    public class ChatbotRespuesta
    {
        public string Tipo { get; set; } = "texto";
        public string? Mensaje { get; set; }
        public object? Datos { get; set; }
        public List<ChatbotOpcion>? Opciones { get; set; }
    }
}