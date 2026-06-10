using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class CarritoItemDb
    {
        [Key]
        public int CarritoItem_Id { get; set; }

        [ForeignKey(nameof(Cliente))]
        public int Cliente_Id { get; set; }
        public Cliente? Cliente { get; set; }

        public int Producto_Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Imagen_URL { get; set; }
        public string? Marca { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public bool Receta { get; set; }
        public DateTime Fecha_Agregado { get; set; } = DateTime.Now;
    }
}