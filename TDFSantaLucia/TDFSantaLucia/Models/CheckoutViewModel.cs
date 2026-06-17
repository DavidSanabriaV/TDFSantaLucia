using System.ComponentModel.DataAnnotations;

namespace TDFSantaLucia.Models
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Seleccione tipo de entrega")]
        public string Tipo_Entrega { get; set; } = "Tienda";

        public string? Direccion_Entrega { get; set; }

        [Required(ErrorMessage = "Ingrese un teléfono de contacto")]
        public string Telefono_Contacto { get; set; } = string.Empty;

        public string? Metodo_Pago { get; set; }

        public string? Receta_URL { get; set; }

        public IFormFile? ArchivoReceta { get; set; }
        public bool RequiereReceta { get; set; }

        public bool Canjear_Puntos { get; set; }
        public int Puntos_Disponibles { get; set; }
        public int Puntos_A_Canjear { get; set; }
        public decimal Descuento_Puntos => Canjear_Puntos && Puntos_A_Canjear > 0
            ? Puntos_A_Canjear
            : 0;

        // ── Cupón ─────────────────────────────────────────────────────────
        public int? Cupon_Id { get; set; }
        public string? CuponCodigo { get; set; }
        public decimal Descuento_Cupon { get; set; } = 0;
        public int ClienteCuponId { get; set; }

        public List<CarritoItem> Items { get; set; } = new();

        public decimal Subtotal => Items.Sum(i => i.Subtotal);
        public decimal Impuesto => Math.Round(Subtotal * 0.13m, 2);
        public decimal TotalSinDescuento => Subtotal + Impuesto;
        public decimal Total => Math.Max(0, TotalSinDescuento - Descuento_Puntos - Descuento_Cupon);

        public bool TieneProductosConReceta => Items.Any(i => i.Receta);
    }
}