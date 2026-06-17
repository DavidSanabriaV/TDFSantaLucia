using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public interface IArticuloRepository
    {
        List<ArticuloSalud> ObtenerTodos(bool soloPublicados = false);
        List<ArticuloSalud> ObtenerPorCategoria(string categoria);
        ArticuloSalud? ObtenerPorId(int id);
        void Agregar(ArticuloSalud articulo);
        void Actualizar(ArticuloSalud articulo);
        void Eliminar(int id);

        LikeArticulo? ObtenerLike(int articuloId, string usuarioId);
        void AgregarLike(LikeArticulo like);
        void EliminarLike(LikeArticulo like);
        int ContarLikes(int articuloId);

        ComentarioSalud? ObtenerComentarioPorId(int comentarioId);
        void AgregarComentario(ComentarioSalud comentario);
        void EliminarComentario(ComentarioSalud comentario);

        void GuardarCambios();
        Task GuardarCambiosAsync();
    }
}