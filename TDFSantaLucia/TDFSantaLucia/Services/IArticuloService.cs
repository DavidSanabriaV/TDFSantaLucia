using TDFSantaLucia.Models;

namespace TDFSantaLucia.Services
{
    public interface IArticuloService
    {
        List<ArticuloSalud> ObtenerTodos(bool soloPublicados = false);
        List<ArticuloSalud> ObtenerPorCategoria(string categoria);
        ArticuloSalud? ObtenerPorId(int id);
        Task<(bool exito, string? error)> CrearAsync(
            ArticuloSalud articulo, string usuarioId);
        Task<(bool exito, string? error)> ActualizarAsync(
            ArticuloSalud articulo);
        Task<(bool exito, string? error)> EliminarAsync(int id);
        Task<(bool exito, string? error)> TogglePublicadoAsync(int id);
        Task<(bool exito, int totalLikes, bool usuarioDioLike)> ToggleLikeAsync(
            int articuloId, string usuarioId);
        Task<(bool exito, string? error, ComentarioSalud? comentario)>
            AgregarComentarioAsync(
                int articuloId, string contenido, string usuarioId);
        Task<(bool exito, string? error)> EliminarComentarioAsync(
            int comentarioId, string usuarioId, bool esAdmin);
    }
}