using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class Producto
    {
        [Key]
        public int Producto_Id { get; set; }

        [ForeignKey(nameof(Categoria))]
        public int Categoria_Id { get; set; }
        public Categoria? Categoria { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string? Descripcion { get; set; }

        public decimal Precio { get; set; }

        public string? Marca { get; set; }

        public bool Estado { get; set; }

        public string? Imagen_URL { get; set; }

        public bool Receta { get; set; }

        public List<Inventario> Inventarios { get; set; } = new();
        public List<DetallePedido> DetallesPedido { get; set; } = new();
        public List<DetalleFactura> DetallesFactura { get; set; } = new();
    }
}