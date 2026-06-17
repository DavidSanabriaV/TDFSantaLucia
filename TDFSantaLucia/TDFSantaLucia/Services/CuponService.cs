using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Data;
using TDFSantaLucia.Models;
using TDFSantaLucia.Repositories;

namespace TDFSantaLucia.Services
{
    public class CuponService : ICuponService
    {
        private readonly ICuponRepository _repo;
        private readonly UserManager<Usuario> _userManager;
        private readonly AppDbContext _db;

        public CuponService(
            ICuponRepository repo,
            UserManager<Usuario> userManager,
            AppDbContext db)
        {
            _repo = repo;
            _userManager = userManager;
            _db = db;
        }

        public List<Cupon> ObtenerTodos() => _repo.ObtenerTodos();
        public Cupon? ObtenerPorId(int id) => _repo.ObtenerPorId(id);
        public List<ClienteCupon> ObtenerCuponesCliente(int clienteId)
            => _repo.ObtenerCuponesCliente(clienteId);

        public (bool exito, string? error) CrearCupon(CuponViewModel model, string usuarioId)
        {
            if (model.Fecha_Expiracion.Date <= DateTime.Today)
                return (false, "La fecha de expiración debe ser futura.");

            if (model.Tipo_Descuento == "Porcentaje" && model.Valor_Descuento > 100)
                return (false, "El porcentaje no puede ser mayor a 100.");

            var cupon = new Cupon
            {
                Descripcion = model.Descripcion.Trim(),
                Tipo_Descuento = model.Tipo_Descuento,
                Valor_Descuento = model.Valor_Descuento,
                Fecha_Expiracion = model.Fecha_Expiracion,
                Estado = model.Estado,
                Fecha_Creacion = DateTime.Now,
                Usuario_Id = usuarioId
            };

            _repo.Agregar(cupon);
            return (true, null);
        }

        public (bool exito, string? error) ActualizarCupon(CuponViewModel model)
        {
            var cupon = _repo.ObtenerPorId(model.Cupon_Id);
            if (cupon == null) return (false, "Cupón no encontrado.");

            if (model.Tipo_Descuento == "Porcentaje" && model.Valor_Descuento > 100)
                return (false, "El porcentaje no puede ser mayor a 100.");

            cupon.Descripcion = model.Descripcion.Trim();
            cupon.Tipo_Descuento = model.Tipo_Descuento;
            cupon.Valor_Descuento = model.Valor_Descuento;
            cupon.Fecha_Expiracion = model.Fecha_Expiracion;
            cupon.Estado = model.Estado;

            _repo.Actualizar(cupon);
            return (true, null);
        }

        public (bool exito, string? error) EliminarCupon(int id)
        {
            var cupon = _repo.ObtenerPorId(id);
            if (cupon == null) return (false, "Cupón no encontrado.");

            if (cupon.ClienteCupones.Any(cc => cc.Utilizado))
                return (false, "No se puede eliminar un cupón que ya fue utilizado.");

            _repo.Eliminar(id);
            return (true, null);
        }

        public (bool exito, string? error) AsignarCuponACliente(int cuponId, int clienteId)
        {
            var cupon = _repo.ObtenerPorId(cuponId);
            if (cupon == null) return (false, "Cupón no encontrado.");
            if (!cupon.Estado || cupon.Fecha_Expiracion.Date < DateTime.Today)
                return (false, "El cupón no está activo o ya venció.");

            var yaExiste = _db.ClientesCupones
                .Any(cc => cc.Cupon_Id == cuponId && cc.Cliente_Id == clienteId);
            if (yaExiste) return (false, "El cliente ya tiene este cupón asignado.");

            _repo.AsignarCuponACliente(new ClienteCupon
            {
                Cliente_Id = clienteId,
                Cupon_Id = cuponId,
                Fecha_Asignacion = DateTime.Now,
                Utilizado = false
            });

            return (true, null);
        }

        public (bool exito, string? error) AsignarCuponATodos(int cuponId, string? filtroRol)
        {
            var cupon = _repo.ObtenerPorId(cuponId);
            if (cupon == null) return (false, "Cupón no encontrado.");
            if (!cupon.Estado || cupon.Fecha_Expiracion.Date < DateTime.Today)
                return (false, "El cupón no está activo o ya venció.");

            var clientes = _db.Clientes.Include(c => c.Usuario).ToList();

            if (!string.IsNullOrEmpty(filtroRol))
            {
                var usuariosEnRol = _userManager
                    .GetUsersInRoleAsync(filtroRol).Result
                    .Select(u => u.Id).ToHashSet();
                clientes = clientes
                    .Where(c => usuariosEnRol.Contains(c.Usuario_ID))
                    .ToList();
            }

            var yaAsignados = _db.ClientesCupones
                .Where(cc => cc.Cupon_Id == cuponId)
                .Select(cc => cc.Cliente_Id)
                .ToHashSet();

            int asignados = 0;
            foreach (var cliente in clientes)
            {
                if (yaAsignados.Contains(cliente.Cliente_Id)) continue;

                _db.ClientesCupones.Add(new ClienteCupon
                {
                    Cliente_Id = cliente.Cliente_Id,
                    Cupon_Id = cuponId,
                    Fecha_Asignacion = DateTime.Now,
                    Utilizado = false
                });
                asignados++;
            }

            _db.SaveChanges();
            return (true, $"Cupón asignado a {asignados} cliente(s).");
        }

        public (bool exito, decimal descuento, int clienteCuponId) ValidarYCalcularDescuento(
            int cuponId, int clienteId, decimal total)
        {
            var cc = _repo.ObtenerClienteCupon(clienteId, cuponId);
            if (cc == null || cc.Utilizado) return (false, 0, 0);

            var cupon = cc.Cupon!;
            if (!cupon.Estado || cupon.Fecha_Expiracion.Date < DateTime.Today)
                return (false, 0, 0);

            decimal descuento = cupon.Tipo_Descuento == "Porcentaje"
                ? Math.Round(total * (cupon.Valor_Descuento / 100m), 2)
                : Math.Min(cupon.Valor_Descuento, total);

            return (true, descuento, cc.Cupon_Cliente_Id);
        }

        public void MarcarComoUtilizado(int clienteCuponId)
            => _repo.MarcarComoUtilizado(clienteCuponId);

        public (bool exito, string? error) DevolverCupon(int clienteCuponId)
        {
            var cc = _db.ClientesCupones.Find(clienteCuponId);
            if (cc == null) return (false, "Cupón no encontrado.");

            cc.Utilizado = false;
            cc.Fecha_Uso = null;
            _db.SaveChanges();
            return (true, null);
        }
    }
}