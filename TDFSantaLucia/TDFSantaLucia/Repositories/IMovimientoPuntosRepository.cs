using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public interface IMovimientoPuntosRepository
    {
        List<MovimientoPuntos> ObtenerPorCliente(int clienteId);
        void Agregar(MovimientoPuntos movimiento);
        void MarcarVencidos();
    }
}