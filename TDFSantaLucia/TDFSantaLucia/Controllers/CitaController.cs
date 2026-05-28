using Microsoft.AspNetCore.Mvc;
using TDFSantaLucia.Models;
using TDFSantaLucia.Services;

namespace TDFSantaLucia.Controllers
{
    [Route("cita")]
    public class CitaController : Controller
    {
        private readonly ICitaService _citaService;

        public CitaController(ICitaService citaService)
        {
            _citaService = citaService;
        }

        [HttpGet("agendar")]
        public IActionResult Agendar()
        {
            var model = _citaService.ObtenerViewModel();
            return View(model);
        }

        [HttpPost("agendar")]
        public IActionResult Agendar(CitaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var vm = _citaService.ObtenerViewModel();
                vm.Servicio = model.Servicio;
                vm.Fecha = model.Fecha;
                vm.Observaciones = model.Observaciones;
                vm.Cliente_Id = model.Cliente_Id;
                return View(vm);
            }

            var resultado = _citaService.AgendarCita(model);
            if (!resultado.success)
            {
                ModelState.AddModelError("", resultado.error ?? "Error al agendar");
                var vm = _citaService.ObtenerViewModel();
                return View(vm);
            }

            return RedirectToAction("MisCitas", new { clienteId = model.Cliente_Id });
        }

        [HttpGet("miscitas")]
        public IActionResult MisCitas()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var cliente = _citaService.ObtenerClientePorUsuarioId(userId);

            if (cliente == null)
                return RedirectToAction("Index", "Home");

            var citas = _citaService.ObtenerPorCliente(cliente.Cliente_Id);
            ViewBag.ClienteId = cliente.Cliente_Id;
            return View(citas);
        }

        [HttpGet("detallecliente/{id:int}")]
        public IActionResult DetalleCliente(int id)
        {
            var cita = _citaService.ObtenerPorId(id);
            if (cita == null) return NotFound();
            return View(cita);
        }

        [HttpGet("crear")]
        public IActionResult Crear()
        {
            var model = _citaService.ObtenerViewModel();
            return View(model);
        }

        [HttpPost("crear")]
        public IActionResult Crear(CitaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var vm = _citaService.ObtenerViewModel();
                vm.Servicio = model.Servicio;
                vm.Fecha = model.Fecha;
                vm.Observaciones = model.Observaciones;
                vm.Cliente_Id = model.Cliente_Id;
                vm.Empleado_Id = model.Empleado_Id;
                return View(vm);
            }

            var resultado = _citaService.AgendarCita(model);
            if (!resultado.success)
            {
                TempData["Error"] = resultado.error;
                return RedirectToAction("Index");
            }

            TempData["Success"] = "Cita creada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var citas = _citaService.ObtenerTodas();
            return View(citas);
        }

        [HttpGet("revisar/{id:int}")]
        public IActionResult Revisar(int id)
        {
            var model = _citaService.ObtenerViewModel(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost("asignar/{id:int}")]
        public IActionResult Asignar(int id, int empleadoId)
        {
            var resultado = _citaService.AsignarEmpleado(id, empleadoId);
            if (!resultado.success)
            {
                TempData["Error"] = resultado.error;
            }
            return RedirectToAction("Revisar", new { id });
        }

        [HttpPost("cambiarestado/{id:int}")]
        public IActionResult CambiarEstado(int id, string estado)
        {
            _citaService.CambiarEstado(id, estado);
            return RedirectToAction("Index");
        }

        [HttpGet("detalle/{id:int}")]
        public IActionResult Detalle(int id)
        {
            var cita = _citaService.ObtenerPorId(id);
            if (cita == null) return NotFound();
            return View(cita);
        }

        [HttpPost("eliminar/{id:int}")]
        public IActionResult Eliminar(int id)
        {
            _citaService.EliminarCita(id);
            return RedirectToAction("Index");
        }
    }
}