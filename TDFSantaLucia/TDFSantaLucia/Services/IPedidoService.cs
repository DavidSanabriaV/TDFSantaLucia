using TDFSantaLucia.Models;

namespace TDFSantaLucia.Services
{
    public interface IPedidoService
    {
        List<Pedido> ObtenerTodos();
        List<Pedido> ObtenerPorCliente(int clienteId);
        Pedido? ObtenerPorId(int id);
        Task<(bool exito, string? error, Pedido? pedido)> ProcesarPedidoAsync(
            CheckoutViewModel checkout, int clienteId);
        Task<(bool exito, string? error)> CambiarEstadoAsync(
            int pedidoId, string nuevoEstado);
        (bool exito, string? error) EliminarPedido(int pedidoId);
    }
}