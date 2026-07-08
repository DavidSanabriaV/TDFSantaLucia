using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class Tratamiento
    {
        [Key]
        public int Tratamiento_Id { get; set; }

        public string? Nombre_Medicamento { get; set; }

        public string? Dosis { get; set; }

        public string? Duracion { get; set; }

        public DateTime Fecha_Inicio { get; set; }

        public DateTime Fecha_Fin { get; set; }

        public bool Estado { get; set; }

        public bool Alertas_Activas { get; set; } = true;

        [ForeignKey(nameof(Cliente))]
        public int Cliente_Id { get; set; }
        public Cliente? Cliente { get; set; }

        public List<RecordatorioTratamiento> Recordatorios { get; set; } = new();
    }
}