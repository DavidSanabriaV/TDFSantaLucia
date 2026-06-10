using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public interface IPedidoRepository
    {
        List<Pedido> ObtenerTodos();
        List<Pedido> ObtenerPorCliente(int clienteId);
        Pedido? ObtenerPorId(int id);
        void Agregar(Pedido pedido);
        void Actualizar(Pedido pedido);
        void Eliminar(int id);
        string GenerarNumeroOrden();
    }
}