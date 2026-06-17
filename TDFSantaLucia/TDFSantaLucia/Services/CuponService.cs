using Microsoft.AspNetCore.Identity;
using TDFSantaLucia.Models;
using TDFSantaLucia.Repositories;

namespace TDFSantaLucia.Services
{
    public class CuponService : ICuponService
    {
        private readonly ICuponRepository _repo;
        private readonly UserManager<Usuario> _userManager;

        public CuponService(ICuponRepository repo, UserManager<Usuario> userManager)
        {
            _repo = repo;
            _userManager = userManager;
        }

        public List<Cupon> ObtenerTodos()
            => _repo.ObtenerTodos();

        public Cupon? ObtenerPorId(int id)
            => _repo.ObtenerPorId(id);

        public List<ClienteCupon> ObtenerCuponesCliente(int clienteId)
            => _repo.ObtenerCuponesCliente(clienteId);

        public (bool exito, string? error) CrearCupon(CuponViewModel model, string usuarioId)
        {
            if (model.Fecha_Expiracion <= DateTime.Today)
                return (false, "La fecha de expiración debe ser futura.");

            var cupon = new Cupon
            {
                Descripcion = model.Descripcion,
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
            if (cupon == null)
                return (false, "Cupón no encontrado.");

            cupon.Descripcion = model.Descripcion;
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
            if (cupon == null)
                return (false, "Cupón no encontrado.");

            if (cupon.ClienteCupones.Any(cc => cc.Utilizado))
                return (false, "No se puede eliminar un cupón que ya fue utilizado.");

            _repo.Eliminar(id);
            return (true, null);
        }

        public (bool exito, string? error) AsignarCuponACliente(int cuponId, int clienteId)
        {
            var cupon = _repo.ObtenerPorId(cuponId);
            if (cupon == null)
                return (false, "Cupón no encontrado.");

            if (!cupon.Estado || cupon.Fecha_Expiracion < DateTime.Today)
                return (false, "El cupón no está disponible.");

            var yaAsignado = _repo.ObtenerClienteCupon(clienteId, cuponId);
            if (yaAsignado != null)
                return (false, "El cliente ya tiene este cupón asignado.");

            _repo.AsignarCuponACliente(new ClienteCupon
            {
                Cliente_Id = clienteId,
                Cupon_Id = cuponId,
                Fecha_Asignacion = DateTime.Now,
                Utilizado = false
            });

            return (true, null);
        }

        public (bool exito, decimal descuento, int clienteCuponId) AplicarCupon(
            int cuponId, int clienteId, decimal total)
        {
            var clienteCupon = _repo.ObtenerClienteCupon(clienteId, cuponId);
            if (clienteCupon == null)
                return (false, 0, 0);

            if (clienteCupon.Utilizado)
                return (false, 0, 0);

            var cupon = clienteCupon.Cupon!;
            if (!cupon.Estado || cupon.Fecha_Expiracion < DateTime.Today)
                return (false, 0, 0);

            decimal descuento = cupon.Tipo_Descuento == "Porcentaje"
                ? Math.Round(total * (cupon.Valor_Descuento / 100), 2)
                : Math.Min(cupon.Valor_Descuento, total);

            return (true, descuento, clienteCupon.Cupon_Cliente_Id);
        }

        public void MarcarComoUtilizado(int clienteCuponId)
            => _repo.MarcarComoUtilizado(clienteCuponId);
    }
}