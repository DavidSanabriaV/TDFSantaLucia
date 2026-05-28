using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Data;
using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly AppDbContext _context;

        public ClienteRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Cliente> ObtenerTodos()
            => _context.Clientes
                .Include(c => c.Usuario)
                .OrderBy(c => c.Usuario.Primer_Apellido)
                .ToList();

        public Cliente? ObtenerPorId(int id)
            => _context.Clientes
                .Include(c => c.Usuario)
                .Include(c => c.Pedidos)
                .Include(c => c.Facturas)
                .Include(c => c.Citas)
                .Include(c => c.Expedientes)
                .Include(c => c.Tratamientos)
                .FirstOrDefault(c => c.Cliente_Id == id);

        public bool ExisteId(int id)
            => _context.Clientes.Any(c => c.Cliente_Id == id);

        public void Agregar(Cliente cliente)
        {
            _context.Clientes.Add(cliente);
            _context.SaveChanges();
        }

        public void Actualizar(Cliente cliente)
        {
            _context.Clientes.Update(cliente);
            _context.SaveChanges();
        }

        public void Eliminar(int id)
        {
            var cliente = _context.Clientes.Find(id);
            if (cliente == null) return;

            _context.Clientes.Remove(cliente);
            _context.SaveChanges();
        }
    }
}