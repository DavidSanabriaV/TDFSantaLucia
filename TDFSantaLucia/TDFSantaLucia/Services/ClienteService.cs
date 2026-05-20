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

        public List<Cliente> ObtenerTodos()
            => _clienteRepo.ObtenerTodos();

        public Cliente? ObtenerPorId(int id)
            => _clienteRepo.ObtenerPorId(id);

        public async Task<bool> CrearCliente(ClienteViewModel model)
        {
            var usuario = new Usuario
            {
                UserName = model.Email,
                Email = model.Email,
                Nombre = model.Nombre,
                Primer_Apellido = model.Primer_Apellido,
                Segundo_Apellido = model.Segundo_Apellido,
                Cedula = model.Cedula,
                Telefono = model.Telefono,
                Direccion_Exacta = model.Direccion_Exacta,
                Estado = model.Estado,
                EmailConfirmed = true
            };

            // Se crea sin contraseña
            var resultado = await _userManager.CreateAsync(usuario);
            if (!resultado.Succeeded)
                return false;

            await _userManager.AddToRoleAsync(usuario, Roles.Cliente);

            var cliente = new Cliente
            {
                Usuario_ID = usuario.Id,
                Fecha_Nacimiento = model.Fecha_Nacimiento,
                Puntos_Acumulados = model.Puntos_Acumulados
            };

            _clienteRepo.Agregar(cliente);
            return true;
        }

        public async Task<bool> ActualizarCliente(ClienteViewModel model)
        {
            var cliente = _clienteRepo.ObtenerPorId(model.Cliente_Id);
            if (cliente == null) return false;

            var usuario = await _userManager.FindByIdAsync(cliente.Usuario_ID);
            if (usuario == null) return false;

            usuario.Nombre = model.Nombre;
            usuario.Primer_Apellido = model.Primer_Apellido;
            usuario.Segundo_Apellido = model.Segundo_Apellido;
            usuario.Cedula = model.Cedula;
            usuario.Telefono = model.Telefono;
            usuario.Direccion_Exacta = model.Direccion_Exacta;
            usuario.Estado = model.Estado;
            usuario.Email = model.Email;
            usuario.UserName = model.Email;

            var resultado = await _userManager.UpdateAsync(usuario);
            if (!resultado.Succeeded) return false;

            cliente.Fecha_Nacimiento = model.Fecha_Nacimiento;
            cliente.Puntos_Acumulados = model.Puntos_Acumulados;

            _clienteRepo.Actualizar(cliente);
            return true;
        }

        public bool EliminarCliente(int id)
        {
            var cliente = _clienteRepo.ObtenerPorId(id);
            if (cliente == null) return false;

            _clienteRepo.Eliminar(id);
            return true;
        }
    }
}