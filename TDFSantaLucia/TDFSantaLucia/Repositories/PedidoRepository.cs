using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Data;
using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly AppDbContext _db;

        public PedidoRepository(AppDbContext db) => _db = db;

        public List<Pedido> ObtenerTodos()
            => _db.Pedidos
                .Include(p => p.Cliente).ThenInclude(c => c.Usuario)
                .Include(p => p.DetallesPedido).ThenInclude(d => d.Producto)
                .OrderByDescending(p => p.Fecha_Creacion)
                .ToList();

        public List<Pedido> ObtenerPorCliente(int clienteId)
            => _db.Pedidos
                .Include(p => p.DetallesPedido).ThenInclude(d => d.Producto)
                .Include(p => p.Facturas)
                .Where(p => p.Cliente_Id == clienteId)
                .OrderByDescending(p => p.Fecha_Creacion)
                .ToList();

        public Pedido? ObtenerPorId(int id)
            => _db.Pedidos
                .Include(p => p.Cliente).ThenInclude(c => c.Usuario)
                .Include(p => p.DetallesPedido).ThenInclude(d => d.Producto)
                .Include(p => p.Facturas).ThenInclude(f => f.DetallesFactura)
                .FirstOrDefault(p => p.Pedido_Id == id);

        public void Agregar(Pedido pedido)
        {
            _db.Pedidos.Add(pedido);
            // NO llamamos SaveChanges aquí, lo maneja el service con transacción
        }

        public void Actualizar(Pedido pedido)
        {
            var existente = _db.Pedidos.Local
                .FirstOrDefault(p => p.Pedido_Id == pedido.Pedido_Id);
            if (existente != null)
                _db.Entry(existente).State = EntityState.Detached;

            _db.Pedidos.Update(pedido);
            _db.SaveChanges();
        }

        public void Eliminar(int id)
        {
            var pedido = _db.Pedidos.Find(id);
            if (pedido != null)
            {
                _db.Pedidos.Remove(pedido);
                _db.SaveChanges();
            }
        }

        public string GenerarNumeroOrden()
        {
            var año = DateTime.Now.Year;
            var ultimo = _db.Pedidos
                .Where(p => p.Numero_Orden != null &&
                            p.Numero_Orden.StartsWith($"ORD-{año}"))
                .OrderByDescending(p => p.Pedido_Id)
                .FirstOrDefault();

            int siguiente = 1;
            if (ultimo?.Numero_Orden != null)
            {
                var partes = ultimo.Numero_Orden.Split('-');
                if (partes.Length == 3 && int.TryParse(partes[2], out int n))
                    siguiente = n + 1;
            }

            return $"ORD-{año}-{siguiente:D4}";
        }
    }
}