using Microsoft.AspNetCore.Identity;
using TDFSantaLucia.Constants;
using TDFSantaLucia.Models;
using TDFSantaLucia.Repositories;

namespace TDFSantaLucia.Services
{
    public class CuentaService : ICuentaService
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly IClienteRepository _clienteRepo;

        public CuentaService(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            IClienteRepository clienteRepo)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _clienteRepo = clienteRepo;
        }

        private static string GenerarUsername(string nombre, string primerApellido)
        {
            var texto = $"{nombre.Trim()}.{primerApellido.Trim()}"
                .Normalize(System.Text.NormalizationForm.FormD);

            var sb = new System.Text.StringBuilder();
            foreach (var c in texto)
            {
                var cat = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString()
                .Normalize(System.Text.NormalizationForm.FormC)
                .Replace(" ", "")
                .ToLower();
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> LoginAsync(
            string correo, string password, bool rememberMe)
        {
            var usuario = await _userManager.FindByEmailAsync(correo);

            if (usuario == null)
                return (false, "Correo o contraseña incorrectos.");

            if (!usuario.Estado)
                return (false, "Tu cuenta está inactiva. Contacta al administrador.");

            var result = await _signInManager.PasswordSignInAsync(
                usuario.UserName!, password, rememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
                return (true, null);

            return (false, "Correo o contraseña incorrectos.");
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> RegistrarClienteAsync(RegisterViewModel model)
        {
            // Validar correo duplicado
            var emailNormalizado = model.Email.Trim().ToUpper();
            var emailExistente = _userManager.Users
                .FirstOrDefault(u => u.NormalizedEmail == emailNormalizado);

            if (emailExistente != null)
                return (false, "Ya existe una cuenta con ese correo.");

            // Validar cédula duplicada
            if (!string.IsNullOrWhiteSpace(model.Cedula))
            {
                var cedulaExistente = _userManager.Users
                    .Any(u => u.Cedula == model.Cedula.Trim());

                if (cedulaExistente)
                    return (false, "Ya existe una cuenta registrada con esa cédula.");
            }

            var usernameBase = GenerarUsername(model.Nombre, model.Primer_Apellido);
            var username = usernameBase;
            int contador = 1;

            while (await _userManager.FindByNameAsync(username) != null)
            {
                username = $"{usernameBase}{contador}";
                contador++;
            }

            var usuario = new Usuario
            {
                UserName = username,
                Email = model.Email.Trim(),
                Nombre = model.Nombre.Trim(),
                Primer_Apellido = model.Primer_Apellido.Trim(),
                Segundo_Apellido = model.Segundo_Apellido.Trim(),
                Cedula = model.Cedula?.Trim(),
                Telefono = model.Telefono?.Trim(),
                Direccion_Exacta = model.Direccion_Exacta?.Trim(),
                Estado = true,
                EmailConfirmed = true
            };

            var resultado = await _userManager.CreateAsync(usuario, model.Password);
            if (!resultado.Succeeded)
                return (false, string.Join(", ", resultado.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(usuario, Roles.Cliente);

            var cliente = new Cliente
            {
                Usuario_ID = usuario.Id,
                Fecha_Nacimiento = model.Fecha_Nacimiento,
                Puntos_Acumulados = 0
            };

            _clienteRepo.Agregar(cliente);

            await _signInManager.SignInAsync(usuario, isPersistent: false);

            return (true, null);
        }
    }
}