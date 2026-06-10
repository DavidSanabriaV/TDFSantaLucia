using System.ComponentModel.DataAnnotations;

namespace TDFSantaLucia.Models
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Seleccione tipo de entrega")]
        public string Tipo_Entrega { get; set; } = "Tienda";

        public string? Direccion_Entrega { get; set; }

        [Required(ErrorMessage = "Ingrese un telefono de contacto")]
        public string Telefono_Contacto { get; set; } = string.Empty;

        public string? Metodo_Pago { get; set; }

        public List<CarritoItem> Items { get; set; } = new();

        public decimal Subtotal => Items.Sum(i => i.Subtotal);
        public decimal Impuesto => Math.Round(Subtotal * 0.13m, 2);
        public decimal Total => Subtotal + Impuesto;
    }
}