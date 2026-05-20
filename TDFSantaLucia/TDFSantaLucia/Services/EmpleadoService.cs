using Microsoft.AspNetCore.Identity;
using TDFSantaLucia.Constants;
using TDFSantaLucia.Models;
using TDFSantaLucia.Repositories;

namespace TDFSantaLucia.Services
{
    public class EmpleadoService : IEmpleadoService
    {
        private readonly IEmpleadoRepository _repository;
        private readonly UserManager<Usuario> _userManager;

        private const decimal Seguro = 0.0917m;
        private const decimal Pension = 0.01m;
        private const decimal Impuesto = 0.10m;

        public EmpleadoService(
            IEmpleadoRepository repository,
            UserManager<Usuario> userManager)
        {
            _repository = repository;
            _userManager = userManager;
        }

        public decimal CalcularSalarioNeto(decimal bruto)
        {
            var deducciones = bruto * (Seguro + Pension + Impuesto);
            return bruto - deducciones;
        }

        public List<string> ObtenerRoles()
        {
            return new List<string>
            {
                Roles.Admin,
                Roles.Empleado
            };
        }

        public async Task<List<Empleado>> ObtenerTodosAsync()
        {
            var empleados = _repository.ObtenerTodos();

            foreach (var emp in empleados)
            {
                if (emp.Usuario != null)
                {
                    var roles = await _userManager.GetRolesAsync(emp.Usuario);

                    emp.Usuario.RolNombre =
                        roles.FirstOrDefault() ?? "";
                }
            }

            return empleados;
        }

        public async Task<Empleado?> ObtenerDetalleAsync(int id)
        {
            var empleado = _repository.ObtenerPorId(id);

            if (empleado?.Usuario != null)
            {
                var roles = await _userManager.GetRolesAsync(
                    empleado.Usuario
                );

                empleado.Usuario.RolNombre =
                    roles.FirstOrDefault() ?? "";
            }

            return empleado;
        }

        public async Task<EmpleadoViewModel?> ObtenerEmpleadoViewModelAsync(int id)
        {
            var empleado = _repository.ObtenerPorId(id);

            if (empleado == null)
                return null;

            var roles = empleado.Usuario != null
                ? await _userManager.GetRolesAsync(empleado.Usuario)
                : new List<string>();

            return new EmpleadoViewModel
            {
                Empleado_Id = empleado.Empleado_Id,
                UsuarioId = empleado.Usuario_ID,

                Nombre = empleado.Usuario.Nombre,
                Primer_Apellido = empleado.Usuario.Primer_Apellido,
                Segundo_Apellido = empleado.Usuario.Segundo_Apellido,

                UserName = empleado.Usuario.UserName,
                Email = empleado.Usuario.Email,

                Cedula = empleado.Usuario.Cedula,
                Telefono = empleado.Usuario.Telefono,
                Direccion_Exacta = empleado.Usuario.Direccion_Exacta,

                rol = roles.FirstOrDefault() ?? "",

                Puesto = empleado.Puesto,

                SalarioBruto = empleado.SalarioBruto,
                SalarioNeto = empleado.SalarioNeto,

                Estado = empleado.Estado
            };
        }

        public async Task<(bool success, string? error)> CrearEmpleadoAsync(
            EmpleadoViewModel model)
        {
            var existe = await _userManager.FindByNameAsync(
                model.UserName
            );

            if (existe != null)
                return (false, "Ya existe un usuario con ese username");

            var usuario = new Usuario
            {
                Nombre = model.Nombre,
                Primer_Apellido = model.Primer_Apellido,
                Segundo_Apellido = model.Segundo_Apellido,

                UserName = model.UserName,
                Email = model.Email,

                Cedula = model.Cedula,
                Telefono = model.Telefono,
                Direccion_Exacta = model.Direccion_Exacta,

                Estado = model.Estado,

                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(
                usuario,
                model.password
            );

            if (!result.Succeeded)
            {
                return (
                    false,
                    string.Join(
                        ", ",
                        result.Errors.Select(e => e.Description)
                    )
                );
            }

            await _userManager.AddToRoleAsync(
                usuario,
                model.rol
            );

            var empleado = new Empleado
            {
                Usuario_ID = usuario.Id,

                Cedula = model.Cedula,
                Telefono = model.Telefono,
                Direccion_Exacta = model.Direccion_Exacta,

                Puesto = model.Puesto,

                SalarioBruto = model.SalarioBruto,
                SalarioNeto = CalcularSalarioNeto(
                    model.SalarioBruto ?? 0
                ),

                Estado = model.Estado
            };

            _repository.Agregar(empleado);

            return (true, null);
        }

        public async Task<(bool success, string? error)> ActualizarEmpleadoAsync(
            EmpleadoViewModel model)
        {
            var existente = await _userManager.FindByIdAsync(
                model.UsuarioId
            );

            if (existente == null)
                return (false, "Usuario no encontrado");

            if (existente.UserName != model.UserName)
            {
                var existe = await _userManager.FindByNameAsync(
                    model.UserName
                );

                if (existe != null)
                    return (false, "Ya existe ese username");
            }

            existente.Nombre = model.Nombre;
            existente.Primer_Apellido = model.Primer_Apellido;
            existente.Segundo_Apellido = model.Segundo_Apellido;

            existente.UserName = model.UserName;
            existente.Email = model.Email;

            existente.Cedula = model.Cedula;
            existente.Telefono = model.Telefono;
            existente.Direccion_Exacta = model.Direccion_Exacta;

            existente.Estado = model.Estado;

            var result = await _userManager.UpdateAsync(existente);

            if (!result.Succeeded)
                return (false, "Error al actualizar usuario");

            var rolesActuales =
                await _userManager.GetRolesAsync(existente);

            await _userManager.RemoveFromRolesAsync(
                existente,
                rolesActuales
            );

            await _userManager.AddToRoleAsync(
                existente,
                model.rol
            );

            if (!string.IsNullOrWhiteSpace(model.password))
            {
                var token =
                    await _userManager.GeneratePasswordResetTokenAsync(
                        existente
                    );

                var passResult =
                    await _userManager.ResetPasswordAsync(
                        existente,
                        token,
                        model.password
                    );

                if (!passResult.Succeeded)
                    return (false, "Error al cambiar contraseña");
            }

            var empleado = new Empleado
            {
                Empleado_Id = model.Empleado_Id,
                Usuario_ID = model.UsuarioId,

                Cedula = model.Cedula,
                Telefono = model.Telefono,
                Direccion_Exacta = model.Direccion_Exacta,

                Puesto = model.Puesto,

                SalarioBruto = model.SalarioBruto,
                SalarioNeto = CalcularSalarioNeto(
                    model.SalarioBruto ?? 0
                ),

                Estado = model.Estado
            };

            _repository.Actualizar(empleado);

            return (true, null);
        }

        public bool EliminarEmpleado(int id)
        {
            var empleado = _repository.ObtenerPorId(id);

            if (empleado == null)
                return false;

            _repository.Eliminar(id);

            return true;
        }
    }
}

