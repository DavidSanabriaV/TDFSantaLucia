using TDFSantaLucia.Models;

namespace TDFSantaLucia.Services
{
    public interface IPuntosService
    {
        int ObtenerPuntosDisponibles(int clienteId);
        List<MovimientoPuntos> ObtenerHistorial(int clienteId);
        (bool valido, string? error) ValidarCanje(int clienteId, int puntosACanjear);
    }
}