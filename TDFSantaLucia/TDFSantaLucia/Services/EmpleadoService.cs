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
            => new List<string> { Roles.Admin, Roles.Empleado };

        private static string GenerarUsername(string nombre, string primerApellido)
        {
            var texto = $"{nombre.Trim()}.{primerApellido.Trim()}"
                .Normalize(System.Text.NormalizationForm.FormD);

            var sb = new System.Text.StringBuilder();
            foreach (var c in texto)
            {
                var cat = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString()
                .Normalize(System.Text.NormalizationForm.FormC)
                .Replace(" ", "")
                .ToLower();
        }

        public async Task<List<Empleado>> ObtenerTodosAsync()
        {
            var empleados = _repository.ObtenerTodos();

            foreach (var emp in empleados)
            {
                if (emp.Usuario != null)
                {
                    var roles = await _userManager.GetRolesAsync(emp.Usuario);
                    emp.Usuario.RolNombre = roles.FirstOrDefault() ?? "";
                }
            }

            return empleados;
        }

        public async Task<Empleado?> ObtenerDetalleAsync(int id)
        {
            var empleado = _repository.ObtenerPorId(id);

            if (empleado?.Usuario != null)
            {
                var roles = await _userManager.GetRolesAsync(empleado.Usuario);
                empleado.Usuario.RolNombre = roles.FirstOrDefault() ?? "";
            }

            return empleado;
        }

        public async Task<EmpleadoViewModel?> ObtenerEmpleadoViewModelAsync(int id)
        {
            var empleado = _repository.ObtenerPorId(id);
            if (empleado == null) return null;

            var roles = empleado.Usuario != null
                ? await _userManager.GetRolesAsync(empleado.Usuario)
                : new List<string>();

            return new EmpleadoViewModel
            {
                Empleado_Id = empleado.Empleado_Id,
                UsuarioId = empleado.Usuario_ID,
                Nombre = empleado.Usuario?.Nombre,
                Primer_Apellido = empleado.Usuario?.Primer_Apellido,
                Segundo_Apellido = empleado.Usuario?.Segundo_Apellido,
                UserName = empleado.Usuario?.UserName,
                Email = empleado.Usuario?.Email,
                Cedula = empleado.Usuario?.Cedula,
                Telefono = empleado.Usuario?.Telefono,
                Direccion_Exacta = empleado.Usuario?.Direccion_Exacta,
                rol = roles.FirstOrDefault() ?? "",
                Puesto = empleado.Puesto,
                SalarioBruto = empleado.SalarioBruto,
                SalarioNeto = empleado.SalarioNeto,
                Estado = empleado.Estado
            };
        }

        public async Task<(bool success, string? error)> CrearEmpleadoAsync(EmpleadoViewModel model)
        {
            var usernameBase = GenerarUsername(model.Nombre, model.Primer_Apellido);
            var username = usernameBase;
            int contador = 1;

            while (await _userManager.FindByNameAsync(username) != null)
            {
                username = $"{usernameBase}{contador}";
                contador++;
            }

            var emailNormalizado = model.Email.Trim().ToUpper();
            var emailExistente = _userManager.Users
                .FirstOrDefault(u => u.NormalizedEmail == emailNormalizado);

            if (emailExistente != null)
                return (false, "Ya existe un usuario registrado con ese correo.");

            var usuario = new Usuario
            {
                Nombre = model.Nombre.Trim(),
                Primer_Apellido = model.Primer_Apellido.Trim(),
                Segundo_Apellido = model.Segundo_Apellido.Trim(),
                UserName = username,
                Email = model.Email.Trim(),
                Cedula = model.Cedula?.Trim(),
                Telefono = model.Telefono?.Trim(),
                Direccion_Exacta = model.Direccion_Exacta?.Trim(),
                Estado = model.Estado,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(
                usuario,
                model.password ?? "Empleado123!"
            );

            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(usuario, model.rol);

            var empleado = new Empleado
            {
                Usuario_ID = usuario.Id,
                Cedula = model.Cedula?.Trim(),
                Telefono = model.Telefono?.Trim(),
                Direccion_Exacta = model.Direccion_Exacta?.Trim(),
                Puesto = model.Puesto?.Trim(),
                SalarioBruto = model.SalarioBruto,
                SalarioNeto = CalcularSalarioNeto(model.SalarioBruto ?? 0),
                Estado = model.Estado
            };

            _repository.Agregar(empleado);

            return (true, null);
        }

        public async Task<(bool success, string? error)> ActualizarEmpleadoAsync(EmpleadoViewModel model)
        {
            var existente = await _userManager.FindByIdAsync(model.UsuarioId);
            if (existente == null)
                return (false, "Usuario no encontrado.");

            if (existente.UserName != model.UserName)
            {
                var existe = await _userManager.FindByNameAsync(model.UserName);
                if (existe != null)
                    return (false, "Ya existe un usuario con ese username.");
            }

            if (existente.Email?.ToUpper() != model.Email.Trim().ToUpper())
            {
                var emailNormalizado = model.Email.Trim().ToUpper();
                var emailExiste = _userManager.Users
                    .FirstOrDefault(u => u.NormalizedEmail == emailNormalizado
                                      && u.Id != existente.Id);

                if (emailExiste != null)
                    return (false, "Ya existe un usuario con ese correo.");
            }

            existente.Nombre = model.Nombre.Trim();
            existente.Primer_Apellido = model.Primer_Apellido.Trim();
            existente.Segundo_Apellido = model.Segundo_Apellido.Trim();
            existente.UserName = model.UserName;
            existente.Email = model.Email.Trim();
            existente.Cedula = model.Cedula?.Trim();
            existente.Telefono = model.Telefono?.Trim();
            existente.Direccion_Exacta = model.Direccion_Exacta?.Trim();
            existente.Estado = model.Estado;

            var result = await _userManager.UpdateAsync(existente);
            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

            var rolesActuales = await _userManager.GetRolesAsync(existente);
            await _userManager.RemoveFromRolesAsync(existente, rolesActuales);
            await _userManager.AddToRoleAsync(existente, model.rol);

            if (!string.IsNullOrWhiteSpace(model.password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(existente);
                var passResult = await _userManager.ResetPasswordAsync(existente, token, model.password);

                if (!passResult.Succeeded)
                    return (false, string.Join(", ", passResult.Errors.Select(e => e.Description)));
            }

            var empleado = new Empleado
            {
                Empleado_Id = model.Empleado_Id,
                Usuario_ID = model.UsuarioId,
                Cedula = model.Cedula?.Trim(),
                Telefono = model.Telefono?.Trim(),
                Direccion_Exacta = model.Direccion_Exacta?.Trim(),
                Puesto = model.Puesto?.Trim(),
                SalarioBruto = model.SalarioBruto,
                SalarioNeto = CalcularSalarioNeto(model.SalarioBruto ?? 0),
                Estado = model.Estado
            };

            _repository.Actualizar(empleado);

            return (true, null);
        }

        public async Task<bool> EliminarEmpleadoAsync(int id)
        {
            var empleado = _repository.ObtenerPorId(id);
            if (empleado == null) return false;

            if (empleado.Horarios?.Any() == true)
                throw new InvalidOperationException("No se puede eliminar un empleado con horarios asignados.");

            if (empleado.Citas?.Any() == true)
                throw new InvalidOperationException("No se puede eliminar un empleado con citas asignadas.");

            if (empleado.Expedientes?.Any() == true)
                throw new InvalidOperationException("No se puede eliminar un empleado con expedientes asignados.");

            var usuarioId = empleado.Usuario_ID;

            _repository.Eliminar(id);

            if (!string.IsNullOrWhiteSpace(usuarioId))
            {
                var usuario = await _userManager.FindByIdAsync(usuarioId);
                if (usuario != null)
                    await _userManager.DeleteAsync(usuario);
            }

            return true;
        }
    }
}