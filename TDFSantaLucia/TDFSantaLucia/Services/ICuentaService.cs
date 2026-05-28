namespace TDFSantaLucia.Services
{
    public interface ICuentaService
    {
        Task<(bool Succeeded, string? ErrorMessage)> LoginAsync(string username, string password, bool rememberMe);
        Task LogoutAsync();
    }
}