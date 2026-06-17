using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Data;
using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public class ArticuloRepository : IArticuloRepository
    {
        private readonly AppDbContext _db;

        public ArticuloRepository(AppDbContext db) => _db = db;

        public List<ArticuloSalud> ObtenerTodos(bool soloPublicados = false)
        {
            var q = _db.ArticulosSalud
                .Include(a => a.Usuario)
                .Include(a => a.Comentarios).ThenInclude(c => c.Usuario)
                .Include(a => a.Likes)
                .AsQueryable();

            if (soloPublicados)
                q = q.Where(a => a.Publicado);

            return q.OrderByDescending(a => a.Fecha_Creacion).ToList();
        }

        public List<ArticuloSalud> ObtenerPorCategoria(string categoria)
            => _db.ArticulosSalud
                .Include(a => a.Usuario)
                .Include(a => a.Likes)
                .Include(a => a.Comentarios)
                .Where(a => a.Publicado && a.Categoria == categoria)
                .OrderByDescending(a => a.Fecha_Creacion)
                .ToList();

        public ArticuloSalud? ObtenerPorId(int id)
            => _db.ArticulosSalud
                .Include(a => a.Usuario)
                .Include(a => a.Comentarios)
                    .ThenInclude(c => c.Usuario)
                .Include(a => a.Likes)
                .FirstOrDefault(a => a.Articulo_Id == id);

        public void Agregar(ArticuloSalud articulo)
        {
            _db.ArticulosSalud.Add(articulo);
            _db.SaveChanges();
        }

        public void Actualizar(ArticuloSalud articulo)
        {
            _db.ArticulosSalud.Update(articulo);
            _db.SaveChanges();
        }

        public void Eliminar(int id)
        {
            var articulo = _db.ArticulosSalud.Find(id);
            if (articulo != null)
            {
                _db.ArticulosSalud.Remove(articulo);
                _db.SaveChanges();
            }
        }


        public LikeArticulo? ObtenerLike(int articuloId, string usuarioId)
            => _db.LikesArticulos
                .FirstOrDefault(l => l.Articulo_Id == articuloId
                                  && l.Usuario_Id == usuarioId);

        public void AgregarLike(LikeArticulo like)
        {
            _db.LikesArticulos.Add(like);
            _db.SaveChanges();
        }

        public void EliminarLike(LikeArticulo like)
        {
            _db.LikesArticulos.Remove(like);
            _db.SaveChanges();
        }

        public int ContarLikes(int articuloId)
            => _db.LikesArticulos.Count(l => l.Articulo_Id == articuloId);


        public ComentarioSalud? ObtenerComentarioPorId(int comentarioId)
            => _db.ComentariosSalud
                .Include(c => c.Usuario)
                .FirstOrDefault(c => c.Comentario_Id == comentarioId);

        public void AgregarComentario(ComentarioSalud comentario)
        {
            _db.ComentariosSalud.Add(comentario);
            _db.SaveChanges();
        }

        public void EliminarComentario(ComentarioSalud comentario)
        {
            _db.ComentariosSalud.Remove(comentario);
            _db.SaveChanges();
        }


        public void GuardarCambios()
            => _db.SaveChanges();

        public async Task GuardarCambiosAsync()
            => await _db.SaveChangesAsync();
    }
}