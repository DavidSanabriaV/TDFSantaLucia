using System.ComponentModel.DataAnnotations;

namespace TDFSantaLucia.Validators
{
    public class FechaNoFuturaAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return true;

            if (value is DateTime fecha)
                return fecha.Date <= DateTime.Today;

            return true;
        }
    }
}