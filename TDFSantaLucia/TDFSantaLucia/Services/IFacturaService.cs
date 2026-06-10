using TDFSantaLucia.Models;

namespace TDFSantaLucia.Services
{
    public interface IFacturaService
    {
        List<Factura> ObtenerTodas();
        List<Factura> ObtenerPorCliente(int clienteId);
        Factura? ObtenerPorId(int id);
        Factura? ObtenerPorPedido(int pedidoId);
        List<Factura> FiltrarPorFecha(List<Factura> facturas,
            DateTime? desde, DateTime? hasta);
        byte[] GenerarPdf(Factura factura);
    }
}