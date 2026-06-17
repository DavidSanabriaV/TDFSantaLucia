using System.ComponentModel.DataAnnotations;

namespace TDFSantaLucia.Models
{
    public class CuponViewModel
    {
        public int Cupon_Id { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El tipo de descuento es obligatorio")]
        public string Tipo_Descuento { get; set; }

        [Required(ErrorMessage = "El valor es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El valor debe ser mayor a 0")]
        public decimal Valor_Descuento { get; set; }

        [Required(ErrorMessage = "La fecha de expiración es obligatoria")]
        public DateTime Fecha_Expiracion { get; set; }

        public bool Estado { get; set; } = true;

        public string? CodigoUsuario { get; set; }
    }
}