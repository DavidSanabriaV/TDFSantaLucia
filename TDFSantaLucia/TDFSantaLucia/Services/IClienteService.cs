using TDFSantaLucia.Models;

namespace TDFSantaLucia.Services
{
    public interface IClienteService
    {
        List<Cliente> ObtenerTodos();
        Cliente? ObtenerPorId(int id);
        Task<bool> CrearCliente(ClienteViewModel model);
        Task<bool> ActualizarCliente(ClienteViewModel model);
        bool EliminarCliente(int id);
    }
}