using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public interface IFacturaRepository
    {
        List<Factura> ObtenerTodas();
        List<Factura> ObtenerPorCliente(int clienteId);
        Factura? ObtenerPorId(int id);
        Factura? ObtenerPorPedido(int pedidoId);
        void Agregar(Factura factura);
        string GenerarNumeroFactura();
    }
}