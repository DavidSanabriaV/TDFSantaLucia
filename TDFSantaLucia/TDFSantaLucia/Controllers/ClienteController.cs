using Microsoft.AspNetCore.Mvc;
using TDFSantaLucia.Models;
using TDFSantaLucia.Services;

namespace TDFSantaLucia.Controllers
{
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

            var resultado = await _clienteService.CrearCliente(model);
            if (!resultado)
            {
                ModelState.AddModelError("", "Error al crear el cliente. Verifique que el correo no esté en uso.");
                return View(model);
            }

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
                Nombre = cliente.Usuario.Nombre,
                Primer_Apellido = cliente.Usuario.Primer_Apellido,
                Segundo_Apellido = cliente.Usuario.Segundo_Apellido,
                Email = cliente.Usuario.Email,
                Cedula = cliente.Usuario.Cedula,
                Telefono = cliente.Usuario.Telefono,
                Direccion_Exacta = cliente.Usuario.Direccion_Exacta,
                Estado = cliente.Usuario.Estado,
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

            var resultado = await _clienteService.ActualizarCliente(model);
            if (!resultado)
            {
                ModelState.AddModelError("", "Error al actualizar el cliente.");
                return View(model);
            }

            return RedirectToAction("Index");
        }

        [HttpPost("eliminar/{id:int}")]
        public IActionResult Eliminar(int id)
        {
            _clienteService.EliminarCliente(id);
            return RedirectToAction("Index");
        }
    }
}