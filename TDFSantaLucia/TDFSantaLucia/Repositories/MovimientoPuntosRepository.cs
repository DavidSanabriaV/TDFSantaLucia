using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Data;
using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public class MovimientoPuntosRepository : IMovimientoPuntosRepository
    {
        private readonly AppDbContext _db;

        public MovimientoPuntosRepository(AppDbContext db) => _db = db;

        public List<MovimientoPuntos> ObtenerPorCliente(int clienteId)
            => _db.MovimientosPuntos
                .Where(m => m.Cliente_Id == clienteId)
                .OrderByDescending(m => m.Fecha)
                .ToList();

        public void Agregar(MovimientoPuntos movimiento)
        {
            _db.MovimientosPuntos.Add(movimiento);
            _db.SaveChanges();
        }

        public void MarcarVencidos()
        {
            var vencidos = _db.MovimientosPuntos
                .Where(m => !m.Vencido
                         && m.Tipo == "Ganado"
                         && m.Fecha_Vencimiento < DateTime.Today)
                .ToList();

            foreach (var m in vencidos)
                m.Vencido = true;

            _db.SaveChangesAsync();
        }
    }
}