using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Data;
using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public class FacturaRepository : IFacturaRepository
    {
        private readonly AppDbContext _db;

        public FacturaRepository(AppDbContext db) => _db = db;

        public List<Factura> ObtenerTodas()
            => _db.Facturas
                .Include(f => f.Cliente).ThenInclude(c => c.Usuario)
                .Include(f => f.Pedido)
                .Include(f => f.DetallesFactura).ThenInclude(d => d.Producto)
                .OrderByDescending(f => f.Fecha_Emision)
                .ToList();

        public List<Factura> ObtenerPorCliente(int clienteId)
    => _db.Facturas
                .Include(f => f.Pedido)
                .Include(f => f.DetallesFactura).ThenInclude(d => d.Producto)
                .Where(f => f.Cliente_Id == clienteId
                 && f.Pedido != null
                 && f.Pedido.Estado != PedidoEstados.Pendiente)  
                .OrderByDescending(f => f.Fecha_Emision)
                .ToList();

        public Factura? ObtenerPorId(int id)
            => _db.Facturas
                .Include(f => f.Cliente).ThenInclude(c => c.Usuario)
                .Include(f => f.Pedido).ThenInclude(p => p.DetallesPedido)
                .Include(f => f.DetallesFactura).ThenInclude(d => d.Producto)
                .FirstOrDefault(f => f.Factura_Id == id);

        public Factura? ObtenerPorPedido(int pedidoId)
            => _db.Facturas
                .Include(f => f.DetallesFactura).ThenInclude(d => d.Producto)
                .FirstOrDefault(f => f.Pedido_Id == pedidoId);

        public void Agregar(Factura factura)
        {
            _db.Facturas.Add(factura);
        }

        public string GenerarNumeroFactura()
        {
            var año = DateTime.Now.Year;
            var ultima = _db.Facturas
                .Where(f => f.Numero_Factura != null &&
                            f.Numero_Factura.StartsWith($"FAC-{año}"))
                .OrderByDescending(f => f.Factura_Id)
                .FirstOrDefault();

            int siguiente = 1;
            if (ultima?.Numero_Factura != null)
            {
                var partes = ultima.Numero_Factura.Split('-');
                if (partes.Length == 3 && int.TryParse(partes[2], out int n))
                    siguiente = n + 1;
            }

            return $"FAC-{año}-{siguiente:D4}";
        }
    }
}