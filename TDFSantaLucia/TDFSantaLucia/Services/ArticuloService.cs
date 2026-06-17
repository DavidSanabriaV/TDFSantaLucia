using TDFSantaLucia.Models;
using TDFSantaLucia.Repositories;

namespace TDFSantaLucia.Services
{
    public class ArticuloService : IArticuloService
    {
        private readonly IArticuloRepository _repo;

        public ArticuloService(IArticuloRepository repo) => _repo = repo;

        public List<ArticuloSalud> ObtenerTodos(bool soloPublicados = false)
            => _repo.ObtenerTodos(soloPublicados);

        public List<ArticuloSalud> ObtenerPorCategoria(string categoria)
            => _repo.ObtenerPorCategoria(categoria);

        public ArticuloSalud? ObtenerPorId(int id)
            => _repo.ObtenerPorId(id);

        public async Task<(bool exito, string? error)> CrearAsync(
    ArticuloSalud articulo, string usuarioId)
        {
            try
            {
                articulo.Usuario_Id = usuarioId;
                articulo.Fecha_Creacion = DateTime.Now;
                articulo.Fecha_Actualizacion = DateTime.Now;
                _repo.Agregar(articulo);
                return (true, null);
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.InnerException?.Message
                         ?? ex.InnerException?.Message
                         ?? ex.Message;
                return (false, inner);
            }
        }

        public async Task<(bool exito, string? error)> ActualizarAsync(
    ArticuloSalud articulo)
        {
            try
            {
                var original = _repo.ObtenerPorId(articulo.Articulo_Id);
                if (original == null)
                    return (false, "Artículo no encontrado.");

                original.Titulo = articulo.Titulo;
                original.Contenido = articulo.Contenido;
                original.Resumen = articulo.Resumen;
                original.Imagen_URL = articulo.Imagen_URL;
                original.Categoria = articulo.Categoria;
                original.Publicado = articulo.Publicado;
                original.Fecha_Actualizacion = DateTime.Now;

                _repo.Actualizar(original);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool exito, string? error)> EliminarAsync(int id)
        {
            var articulo = _repo.ObtenerPorId(id);
            if (articulo == null)
                return (false, "Artículo no encontrado.");

            _repo.Eliminar(id);
            return (true, null);
        }

        public async Task<(bool exito, string? error)> TogglePublicadoAsync(
            int id)
        {
            var articulo = _repo.ObtenerPorId(id);
            if (articulo == null)
                return (false, "Artículo no encontrado.");

            articulo.Publicado = !articulo.Publicado;
            articulo.Fecha_Actualizacion = DateTime.Now;
            _repo.Actualizar(articulo);
            return (true, null);
        }

        public async Task<(bool exito, int totalLikes, bool usuarioDioLike)>
            ToggleLikeAsync(int articuloId, string usuarioId)
        {
            var like = _repo.ObtenerLike(articuloId, usuarioId);

            if (like != null)
            {
                _repo.EliminarLike(like);
                var total1 = _repo.ContarLikes(articuloId);
                return (true, total1, false);
            }

            _repo.AgregarLike(new LikeArticulo
            {
                Articulo_Id = articuloId,
                Usuario_Id = usuarioId,
                Fecha = DateTime.Now
            });

            var total = _repo.ContarLikes(articuloId);
            return (true, total, true);
        }

        public async Task<(bool exito, string? error,
            ComentarioSalud? comentario)> AgregarComentarioAsync(
                int articuloId, string contenido, string usuarioId)
        {
            var articulo = _repo.ObtenerPorId(articuloId);
            if (articulo == null)
                return (false, "Artículo no encontrado.", null);

            var comentario = new ComentarioSalud
            {
                Articulo_Id = articuloId,
                Contenido = contenido,
                Usuario_Id = usuarioId,
                Fecha_Creacion = DateTime.Now
            };

            _repo.AgregarComentario(comentario);

            var completo = _repo.ObtenerComentarioPorId(
                comentario.Comentario_Id);

            return (true, null, completo);
        }

        public async Task<(bool exito, string? error)> EliminarComentarioAsync(
            int comentarioId, string usuarioId, bool esAdmin)
        {
            var comentario = _repo.ObtenerComentarioPorId(comentarioId);
            if (comentario == null)
                return (false, "Comentario no encontrado.");

            if (!esAdmin && comentario.Usuario_Id != usuarioId)
                return (false,
                    "No tienes permiso para eliminar este comentario.");

            _repo.EliminarComentario(comentario);
            return (true, null);
        }
    }
}