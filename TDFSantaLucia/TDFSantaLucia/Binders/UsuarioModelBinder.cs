using TDFSantaLucia.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TDFSantaLucia.Binders
{
    public class UsuarioModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var request = bindingContext.HttpContext.Request;

            var id = request.Form["Id"].ToString();
            var nombre = request.Form["Nombre"].ToString().Trim();
            var primerApellido = request.Form["Primer_Apellido"].ToString().Trim();
            var segundoApellido = request.Form["Segundo_Apellido"].ToString().Trim();
            var password = request.Form["Password"].ToString();
            var direccion = request.Form["Direccion_Exacta"].ToString().Trim();
            var telefono = request.Form["Telefono"].ToString().Trim();
            var cedula = request.Form["Cedula"].ToString().Trim();
            var correo = request.Form["Correo"].ToString().Trim();
            var email = request.Form["Email"].ToString().Trim();

            bool estado = request.Form["Estado"].Contains("true");

            string username;
            if (!string.IsNullOrWhiteSpace(nombre) && !string.IsNullOrWhiteSpace(primerApellido))
            {
                username = $"{nombre}.{primerApellido}"
                    .Normalize(System.Text.NormalizationForm.FormD)
                    .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                                != System.Globalization.UnicodeCategory.NonSpacingMark)
                    .Aggregate("", (s, c) => s + c)
                    .Replace(" ", "")
                    .ToLower();
            }
            else
            {
                username = Guid.NewGuid().ToString("N");
            }

            var emailFinal = !string.IsNullOrWhiteSpace(email)
                ? email
                : (!string.IsNullOrWhiteSpace(correo) ? correo : null);

            var usuario = new Usuario
            {
                Id = id,
                Nombre = nombre,
                Primer_Apellido = primerApellido,
                Segundo_Apellido = segundoApellido,
                UserName = username,
                NormalizedUserName = username.ToUpper(),
                Email = emailFinal,
                NormalizedEmail = emailFinal?.ToUpper(),
                Estado = estado,
                Direccion_Exacta = string.IsNullOrWhiteSpace(direccion) ? null : direccion,
                Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono,
                Cedula = string.IsNullOrWhiteSpace(cedula) ? null : cedula,
                Correo = string.IsNullOrWhiteSpace(correo) ? null : correo,
            };

            bindingContext.HttpContext.Items["Password"] = password;

            bindingContext.Result = ModelBindingResult.Success(usuario);
            return Task.CompletedTask;
        }
    }
}