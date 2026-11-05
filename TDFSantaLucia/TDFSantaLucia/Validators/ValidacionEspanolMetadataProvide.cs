using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace TDFSantaLucia.Validators
{
    public class ValidacionEspanolMetadataProvider : IValidationMetadataProvider
    {
        public void CreateValidationMetadata(ValidationMetadataProviderContext context)
        {
            foreach (var meta in context.ValidationMetadata.ValidatorMetadata)
            {
                if (meta is not ValidationAttribute attr) continue;

                if (!string.IsNullOrEmpty(attr.ErrorMessage) ||
                    !string.IsNullOrEmpty(attr.ErrorMessageResourceName)) continue;

                attr.ErrorMessage = attr switch
                {
                    RequiredAttribute => "El campo {0} es obligatorio.",
                    StringLengthAttribute s when s.MinimumLength > 0
                        => "El campo {0} debe tener entre {2} y {1} caracteres.",
                    StringLengthAttribute => "El campo {0} no puede superar los {1} caracteres.",
                    MaxLengthAttribute => "El campo {0} no puede superar los {1} caracteres.",
                    MinLengthAttribute => "El campo {0} debe tener al menos {1} caracteres.",
                    RangeAttribute => "El campo {0} debe estar entre {1} y {2}.",
                    EmailAddressAttribute => "El campo {0} no es un correo electrónico válido.",
                    PhoneAttribute => "El campo {0} no es un número de teléfono válido.",
                    UrlAttribute => "El campo {0} no es una URL válida.",
                    CompareAttribute => "El campo {0} debe coincidir con {1}.",
                    RegularExpressionAttribute => "El campo {0} no tiene un formato válido.",
                    _ => "El campo {0} no es válido."
                };
            }
        }
    }
}