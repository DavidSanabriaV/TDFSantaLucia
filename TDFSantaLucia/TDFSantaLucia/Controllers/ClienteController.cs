using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TDFSantaLucia.Models;
using TDFSantaLucia.Services;

namespace TDFSantaLucia.Controllers
{
    [Authorize(Roles = "Admin,Empleado")]
    [Route("cliente")]
    public class ClienteController : Controller
    {
        private readonly IClienteService _clienteService;

        public ClienteController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var clientes = _clienteService.ObtenerTodos();
            return View(clientes);
        }

        [HttpGet("detalle/{id:int}")]
        public IActionResult Detalle(int id)
        {
            var cliente = _clienteService.ObtenerPorId(id);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        [HttpGet("crear")]
        public IActionResult Crear()
        {
            return View(new ClienteViewModel());
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear(ClienteViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (exito, error) = await _clienteService.CrearCliente(model);
            if (!exito)
            {
                ModelState.AddModelError("", error ?? "Error al crear el cliente.");
                return View(model);
            }

            TempData["ExitoCliente"] = "Cliente creado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet("editar/{id:int}")]
        public IActionResult Editar(int id)
        {
            var cliente = _clienteService.ObtenerPorId(id);
            if (cliente == null) return NotFound();

            var model = new ClienteViewModel
            {
                Cliente_Id = cliente.Cliente_Id,
                Usuario_ID = cliente.Usuario_ID,
                Nombre = cliente.Usuario?.Nombre,
                Primer_Apellido = cliente.Usuario?.Primer_Apellido,
                Segundo_Apellido = cliente.Usuario?.Segundo_Apellido,
                Email = cliente.Usuario?.Email,
                Cedula = cliente.Usuario?.Cedula,
                Telefono = cliente.Usuario?.Telefono,
                Direccion_Exacta = cliente.Usuario?.Direccion_Exacta,
                Estado = cliente.Usuario?.Estado ?? true,
                Fecha_Nacimiento = cliente.Fecha_Nacimiento,
                Puntos_Acumulados = cliente.Puntos_Acumulados
            };

            return View(model);
        }

        [HttpPost("editar/{id:int}")]
        public async Task<IActionResult> Editar(ClienteViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (exito, error) = await _clienteService.ActualizarCliente(model);
            if (!exito)
            {
                ModelState.AddModelError("", error ?? "Error al actualizar el cliente.");
                return View(model);
            }

            TempData["Exito"] = "Cliente actualizado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost("activar/{id:int}")]
        public async Task<IActionResult> Activar(int id)
        {
            var (exito, error) = await _clienteService.CambiarEstadoAsync(id, true);
            TempData[exito ? "ExitoCliente" : "ErrorCliente"] =
                exito ? "Cliente activado correctamente." : error;

            return RedirectToAction("Index");
        }

        [HttpPost("desactivar/{id:int}")]
        public async Task<IActionResult> Desactivar(int id)
        {
            var (exito, error) = await _clienteService.CambiarEstadoAsync(id, false);
            TempData[exito ? "ExitoCliente" : "ErrorCliente"] =
                exito ? "Cliente desactivado correctamente." : error;

            return RedirectToAction("Index");
        }
    }
}