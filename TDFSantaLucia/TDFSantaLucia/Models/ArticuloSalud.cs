using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class ArticuloSalud
    {
        [Key]
        public int Articulo_Id { get; set; }

        [Required]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Contenido { get; set; } = string.Empty;

        public string? Resumen { get; set; }

        public string? Imagen_URL { get; set; }

        public string? Categoria { get; set; }

        public bool Publicado { get; set; } = false;

        public DateTime Fecha_Creacion { get; set; } = DateTime.Now;

        public DateTime Fecha_Actualizacion { get; set; } = DateTime.Now;

        [ForeignKey(nameof(Usuario))]
        public string Usuario_Id { get; set; } = string.Empty;
        public Usuario? Usuario { get; set; }

        public List<ComentarioSalud> Comentarios { get; set; } = new();
        public List<LikeArticulo> Likes { get; set; } = new();
    }
}