using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class RecordatorioTratamiento
    {
        [Key]
        public int Recordatorio_Id { get; set; }

        public TimeSpan Hora { get; set; }

        public bool Confirmacion { get; set; }

        public bool Alerta_Activa { get; set; } = true;

        [ForeignKey(nameof(Tratamiento))]
        public int Tratamiento_Id { get; set; }
        public Tratamiento? Tratamiento { get; set; }
    }
}