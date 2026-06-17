using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class LikeArticulo
    {
        [Key]
        public int Like_Id { get; set; }

        [ForeignKey(nameof(Articulo))]
        public int Articulo_Id { get; set; }
        public ArticuloSalud? Articulo { get; set; }

        [ForeignKey(nameof(Usuario))]
        public string Usuario_Id { get; set; } = string.Empty;
        public Usuario? Usuario { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}