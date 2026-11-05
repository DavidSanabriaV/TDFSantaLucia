using Microsoft.AspNetCore.Identity;
using TDFSantaLucia.Models;

namespace TDFSantaLucia.Validations
{
    public class SinEspaciosPasswordValidator : IPasswordValidator<Usuario>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<Usuario> manager, Usuario user, string? password)
        {
            if (!string.IsNullOrEmpty(password) && password.Any(char.IsWhiteSpace))
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "PasswordConEspacios",
                    Description = "La contraseña no puede contener espacios."
                }));
            }

            return Task.FromResult(IdentityResult.Success);
        }
    }
}

