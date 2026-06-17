using TDFSantaLucia.Models;
using TDFSantaLucia.Repositories;

namespace TDFSantaLucia.Services
{
    public class PuntosService : IPuntosService
    {
        private readonly IMovimientoPuntosRepository _repo;

        public PuntosService(IMovimientoPuntosRepository repo)
        {
            _repo = repo;
        }

        public int ObtenerPuntosDisponibles(int clienteId)
        {
            _repo.MarcarVencidos();

            var movimientos = _repo.ObtenerPorCliente(clienteId);

            return movimientos
                .Where(m => !m.Vencido)
                .Sum(m => m.Puntos);
        }

        public List<MovimientoPuntos> ObtenerHistorial(int clienteId)
            => _repo.ObtenerPorCliente(clienteId);

        public (bool valido, string? error) ValidarCanje(
            int clienteId, int puntosACanjear)
        {
            if (puntosACanjear <= 0)
                return (false, "Debe canjear al menos 1 punto.");

            var disponibles = ObtenerPuntosDisponibles(clienteId);

            if (puntosACanjear > disponibles)
                return (false,
                    $"No tienes suficientes puntos. " +
                    $"Disponibles: {disponibles}.");

            return (true, null);
        }
    }
}