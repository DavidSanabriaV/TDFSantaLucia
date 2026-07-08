using System.ComponentModel.DataAnnotations;

namespace TDFSantaLucia.Models
{
    public class TratamientoViewModel
    {
        public int Tratamiento_Id { get; set; }

        [Required(ErrorMessage = "El nombre del medicamento es obligatorio")]
        public string Nombre_Medicamento { get; set; } = string.Empty;

        public string? Dosis { get; set; }

        public string? Duracion { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        public DateTime Fecha_Inicio { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La fecha de fin es obligatoria")]
        public DateTime Fecha_Fin { get; set; } = DateTime.Today.AddDays(7);

        public bool Estado { get; set; } = true;

        public bool Alertas_Activas { get; set; } = true;

        public List<string> Horarios { get; set; } = new(); 
    }
}