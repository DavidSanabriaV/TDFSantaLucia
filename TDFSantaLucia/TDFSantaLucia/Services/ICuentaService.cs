using TDFSantaLucia.Models;

namespace TDFSantaLucia.Services
{
    public interface ICuentaService
    {
        Task<(bool Succeeded, string? ErrorMessage)> LoginAsync(string correo, string password, bool rememberMe);
        Task LogoutAsync();
        Task<(bool Succeeded, string? ErrorMessage)> RegistrarClienteAsync(RegisterViewModel model);
    }
}