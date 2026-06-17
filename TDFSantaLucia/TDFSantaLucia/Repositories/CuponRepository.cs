using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Data;
using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public class CuponRepository : ICuponRepository
    {
        private readonly AppDbContext _db;

        public CuponRepository(AppDbContext db) => _db = db;

        public List<Cupon> ObtenerTodos()
            => _db.Cupones
                .Include(c => c.Usuario)
                .Include(c => c.ClienteCupones)
                    .ThenInclude(cc => cc.Cliente)
                        .ThenInclude(cl => cl.Usuario)
                .OrderByDescending(c => c.Fecha_Creacion)
                .ToList();

        public Cupon? ObtenerPorId(int id)
            => _db.Cupones
                .Include(c => c.Usuario)
                .Include(c => c.ClienteCupones)
                    .ThenInclude(cc => cc.Cliente)
                        .ThenInclude(cl => cl.Usuario)
                .FirstOrDefault(c => c.Cupon_Id == id);

        public List<Cupon> ObtenerDisponibles()
            => _db.Cupones
                .Where(c => c.Estado && c.Fecha_Expiracion >= DateTime.Today)
                .OrderBy(c => c.Fecha_Expiracion)
                .ToList();

        public List<ClienteCupon> ObtenerCuponesCliente(int clienteId)
            => _db.ClientesCupones
                .Include(cc => cc.Cupon)
                .Where(cc => cc.Cliente_Id == clienteId
                          && !cc.Utilizado
                          && cc.Cupon!.Estado
                          && cc.Cupon.Fecha_Expiracion >= DateTime.Today)
                .OrderBy(cc => cc.Cupon!.Fecha_Expiracion)
                .ToList();

        public ClienteCupon? ObtenerClienteCupon(int clienteId, int cuponId)
            => _db.ClientesCupones
                .Include(cc => cc.Cupon)
                .FirstOrDefault(cc => cc.Cliente_Id == clienteId
                                   && cc.Cupon_Id == cuponId
                                   && !cc.Utilizado);

        public void Agregar(Cupon cupon)
        {
            _db.Cupones.Add(cupon);
            _db.SaveChanges();
        }

        public void Actualizar(Cupon cupon)
        {
            var existing = _db.Cupones.Local
                .FirstOrDefault(c => c.Cupon_Id == cupon.Cupon_Id);
            if (existing != null)
                _db.Entry(existing).State = EntityState.Detached;

            _db.Cupones.Update(cupon);
            _db.SaveChanges();
        }

        public void Eliminar(int id)
        {
            var cupon = _db.Cupones.Find(id);
            if (cupon != null)
            {
                _db.Cupones.Remove(cupon);
                _db.SaveChanges();
            }
        }

        public void AsignarCuponACliente(ClienteCupon clienteCupon)
        {
            _db.ClientesCupones.Add(clienteCupon);
            _db.SaveChanges();
        }

        public void MarcarComoUtilizado(int clienteCuponId)
        {
            var cc = _db.ClientesCupones.Find(clienteCuponId);
            if (cc != null)
            {
                cc.Utilizado = true;
                cc.Fecha_Uso = DateTime.Now;
                _db.SaveChanges();
            }
        }
    }
}