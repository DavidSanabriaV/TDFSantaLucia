using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class ComentarioSalud
    {
        [Key]
        public int Comentario_Id { get; set; }

        [Required]
        public string Contenido { get; set; } = string.Empty;

        public DateTime Fecha_Creacion { get; set; } = DateTime.Now;

        [ForeignKey(nameof(Articulo))]
        public int Articulo_Id { get; set; }
        public ArticuloSalud? Articulo { get; set; }

        [ForeignKey(nameof(Usuario))]
        public string Usuario_Id { get; set; } = string.Empty;
        public Usuario? Usuario { get; set; }
    }
}