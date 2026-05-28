using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TDFSantaLucia.Models;
using TDFSantaLucia.Services;

namespace TDFSantaLucia.Controllers
{
    [Route("expediente")]
    public class ExpedienteController : Controller
    {
        private readonly IExpedienteService _expedienteService;
        private readonly IClienteService _clienteService;
        private readonly IEmpleadoService _empleadoService;

        public ExpedienteController(
            IExpedienteService expedienteService,
            IClienteService clienteService,
            IEmpleadoService empleadoService)
        {
            _expedienteService = expedienteService;
            _clienteService = clienteService;
            _empleadoService = empleadoService;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var expedientes = _expedienteService.ObtenerTodos();
            return View(expedientes);
        }

        [HttpGet("detalle/{id:int}")]
        public IActionResult Detalle(int id)
        {
            var expediente = _expedienteService.ObtenerDetalle(id);
            if (expediente == null)
                return NotFound();

            return View(expediente);
        }

        [HttpGet("crear")]
        public async Task<IActionResult> Crear()
        {
            await CargarSelectLists();
            return View(new Expediente());
        }

        [HttpPost("crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Expediente expediente)
        {
            if (!ModelState.IsValid)
            {
                await CargarSelectLists();
                return View(expediente);
            }

            var (exito, error) = _expedienteService.CrearExpediente(expediente);
            if (!exito)
            {
                ModelState.AddModelError(string.Empty, error!);
                await CargarSelectLists();
                return View(expediente);
            }

            TempData["ExitoExpediente"] = "Expediente creado correctamente";
            return RedirectToAction("Index");
        }

        [HttpGet("editar/{id:int}")]
        public async Task<IActionResult> Editar(int id)
        {
            var expediente = _expedienteService.ObtenerDetalle(id);
            if (expediente == null)
                return NotFound();

            await CargarSelectLists(expediente.Cliente_Id, expediente.Empleado_Id);
            return View(expediente);
        }

        [HttpPost("editar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Expediente expediente)
        {
            if (id != expediente.Expediente_Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                await CargarSelectLists(expediente.Cliente_Id, expediente.Empleado_Id);
                return View(expediente);
            }

            var (exito, error) = _expedienteService.ActualizarExpediente(id, expediente);
            if (!exito)
            {
                ModelState.AddModelError(string.Empty, error!);
                await CargarSelectLists(expediente.Cliente_Id, expediente.Empleado_Id);
                return View(expediente);
            }

            TempData["ExitoExpediente"] = "Expediente actualizado correctamente";
            return RedirectToAction("Index");
        }

        [HttpPost("eliminar/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var (exito, error) = _expedienteService.EliminarExpediente(id);
            if (!exito)
                TempData["ErrorExpediente"] = error;
            else
                TempData["ExitoExpediente"] = "Expediente eliminado correctamente";

            return RedirectToAction("Index");
        }

        [HttpGet("mi-expediente/{clienteId:int}")]
        public IActionResult MiExpediente(int clienteId)
        {
            var expedientes = _expedienteService.ObtenerPorCliente(clienteId);
            return View(expedientes);
        }

        private async Task CargarSelectLists(int? clienteId = null, int? empleadoId = null)
        {
            var clientes = _clienteService.ObtenerTodos();
            var empleados = await _empleadoService.ObtenerTodosAsync();

            ViewBag.Clientes = new SelectList(
                clientes.Select(c => new {
                    c.Cliente_Id,
                    NombreCompleto = $"{c.Usuario.Nombre} {c.Usuario.Primer_Apellido} {c.Usuario.Segundo_Apellido}"
                }),
                "Cliente_Id",
                "NombreCompleto",
                clienteId
            );

            ViewBag.Empleados = new SelectList(
                empleados.Select(e => new {
                    e.Empleado_Id,
                    NombreCompleto = $"{e.Usuario.Nombre} {e.Usuario.Primer_Apellido} {e.Usuario.Segundo_Apellido}"
                }),
                "Empleado_Id",
                "NombreCompleto",
                empleadoId
            );
        }
    }
}