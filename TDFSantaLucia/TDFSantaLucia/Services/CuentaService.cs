using Microsoft.AspNetCore.Identity;
using TDFSantaLucia.Models;

namespace TDFSantaLucia.Services
{
    public class CuentaService : ICuentaService
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;

        public CuentaService(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
    }
}