using System.ComponentModel.DataAnnotations;

namespace TDFSantaLucia.Models
{
    public class Categoria
    {
        [Key]
        public int Categoria_Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string? Descripcion { get; set; }

        public bool Estado { get; set; }

        public List<Producto> Productos { get; set; } = new();
    }
}