using Microsoft.AspNetCore.Identity;
using TDFSantaLucia.Constants;
using TDFSantaLucia.Models;
using TDFSantaLucia.Repositories;

namespace TDFSantaLucia.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepo;
        private readonly UserManager<Usuario> _userManager;

        public ClienteService(IClienteRepository clienteRepo, UserManager<Usuario> userManager)
        {
            _clienteRepo = clienteRepo;
            _userManager = userManager;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

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

        // ── Consultas ─────────────────────────────────────────────────────────

        public List<Cliente> ObtenerTodos()
            => _clienteRepo.ObtenerTodos();

        public Cliente? ObtenerPorId(int id)
            => _clienteRepo.ObtenerPorId(id);

        // ── CRUD ──────────────────────────────────────────────────────────────

        public async Task<(bool exito, string? error)> CrearCliente(ClienteViewModel model)
        {
            // Autogenerar username y resolver duplicados
            var usernameBase = GenerarUsername(model.Nombre, model.Primer_Apellido);
            var username = usernameBase;
            int contador = 1;

            while (await _userManager.FindByNameAsync(username) != null)
            {
                username = $"{usernameBase}{contador}";
                contador++;
            }

            // Verificar email duplicado
            var emailNormalizado = model.Email.Trim().ToUpper();
            var emailExistente = _userManager.Users
                .FirstOrDefault(u => u.NormalizedEmail == emailNormalizado);

            if (emailExistente != null)
                return (false, "Ya existe un usuario registrado con ese correo.");

            var usuario = new Usuario
            {
                UserName = username,
                Email = model.Email.Trim(),
                Nombre = model.Nombre.Trim(),
                Primer_Apellido = model.Primer_Apellido.Trim(),
                Segundo_Apellido = model.Segundo_Apellido.Trim(),
                Cedula = model.Cedula?.Trim(),
                Telefono = model.Telefono?.Trim(),
                Direccion_Exacta = model.Direccion_Exacta?.Trim(),
                Estado = model.Estado,
                EmailConfirmed = true
            };

            var resultado = await _userManager.CreateAsync(
                usuario,
                model.Password ?? "Cliente123!"
            );

            if (!resultado.Succeeded)
                return (false, string.Join(", ", resultado.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(usuario, Roles.Cliente);

            var cliente = new Cliente
            {
                Usuario_ID = usuario.Id,
                Fecha_Nacimiento = model.Fecha_Nacimiento,
                Puntos_Acumulados = model.Puntos_Acumulados
            };

            _clienteRepo.Agregar(cliente);
            return (true, null);
        }

        public async Task<(bool exito, string? error)> ActualizarCliente(ClienteViewModel model)
        {
            var cliente = _clienteRepo.ObtenerPorId(model.Cliente_Id);
            if (cliente == null)
                return (false, "Cliente no encontrado.");

            var usuario = await _userManager.FindByIdAsync(cliente.Usuario_ID);
            if (usuario == null)
                return (false, "Usuario no encontrado.");

            // Verificar email duplicado en otro usuario
            if (usuario.Email?.ToUpper() != model.Email.Trim().ToUpper())
            {
                var emailNormalizado = model.Email.Trim().ToUpper();
                var emailExiste = _userManager.Users
                    .FirstOrDefault(u => u.NormalizedEmail == emailNormalizado
                                      && u.Id != usuario.Id);

                if (emailExiste != null)
                    return (false, "Ya existe un usuario con ese correo.");
            }

            usuario.Nombre = model.Nombre.Trim();
            usuario.Primer_Apellido = model.Primer_Apellido.Trim();
            usuario.Segundo_Apellido = model.Segundo_Apellido.Trim();
            usuario.Cedula = model.Cedula?.Trim();
            usuario.Telefono = model.Telefono?.Trim();
            usuario.Direccion_Exacta = model.Direccion_Exacta?.Trim();
            usuario.Estado = model.Estado;
            usuario.Email = model.Email.Trim();

            var resultado = await _userManager.UpdateAsync(usuario);
            if (!resultado.Succeeded)
                return (false, string.Join(", ", resultado.Errors.Select(e => e.Description)));

            // Cambiar contraseña solo si se proporcionó una nueva
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                var passResult = await _userManager.ResetPasswordAsync(usuario, token, model.Password);

                if (!passResult.Succeeded)
                    return (false, string.Join(", ", passResult.Errors.Select(e => e.Description)));
            }

            cliente.Fecha_Nacimiento = model.Fecha_Nacimiento;
            cliente.Puntos_Acumulados = model.Puntos_Acumulados;

            _clienteRepo.Actualizar(cliente);
            return (true, null);
        }

        public async Task<(bool exito, string? error)> EliminarClienteAsync(int id)
        {
            var cliente = _clienteRepo.ObtenerPorId(id);
            if (cliente == null)
                return (false, "Cliente no encontrado.");

            // Validar dependencias
            if (cliente.Pedidos?.Any() == true)
                return (false, "No se puede eliminar un cliente con pedidos registrados.");

            if (cliente.Facturas?.Any() == true)
                return (false, "No se puede eliminar un cliente con facturas registradas.");

            if (cliente.Citas?.Any() == true)
                return (false, "No se puede eliminar un cliente con citas asignadas.");

            if (cliente.Expedientes?.Any() == true)
                return (false, "No se puede eliminar un cliente con expedientes registrados.");

            if (cliente.Tratamientos?.Any() == true)
                return (false, "No se puede eliminar un cliente con tratamientos registrados.");

            var usuarioId = cliente.Usuario_ID;

            // Primero eliminar el cliente
            _clienteRepo.Eliminar(id);

            // Luego eliminar el usuario con UserManager
            if (!string.IsNullOrWhiteSpace(usuarioId))
            {
                var usuario = await _userManager.FindByIdAsync(usuarioId);
                if (usuario != null)
                    await _userManager.DeleteAsync(usuario);
            }

            return (true, null);
        }
    }
}