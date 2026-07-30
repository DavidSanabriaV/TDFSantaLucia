using Microsoft.AspNetCore.Authorization;
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

        [Authorize(Roles = "Cliente")]
        [HttpGet("agendar")]
        public IActionResult Agendar()
        {
            return View(new CitaViewModel());
        }

        [Authorize(Roles = "Cliente")]
        [HttpPost("agendar")]
        public IActionResult Agendar(CitaViewModel model)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var cliente = _citaService.ObtenerClientePorUsuarioId(userId!);

            if (cliente == null)
            {
                ModelState.AddModelError("", "No se encontró el cliente asociado a tu cuenta.");
                return View(model);
            }

            model.Cliente_Id = cliente.Cliente_Id;

            if (!ModelState.IsValid)
                return View(model);

            var resultado = _citaService.AgendarCita(model);
            if (!resultado.success)
            {
                ModelState.AddModelError("", resultado.error ?? "Error al agendar");
                return View(model);
            }

            return RedirectToAction("MisCitas");
        }

        [Authorize(Roles = "Cliente")]
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

        [Authorize(Roles = "Cliente")]
        [HttpGet("detallecliente/{id:int}")]
        public IActionResult DetalleCliente(int id)
        {
            var cita = _citaService.ObtenerPorId(id);
            if (cita == null) return NotFound();
            return View(cita);
        }

        [Authorize(Roles = "Admin,Empleado")]
        [HttpGet("crear")]
        public IActionResult Crear()
        {
            var model = _citaService.ObtenerViewModel();
            return View(model);
        }

        [Authorize(Roles = "Admin,Empleado")]
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
                TempData["ErrorCita"] = resultado.error;
                return RedirectToAction("Index");
            }

            TempData["ExitoCita"] = "Cita creada correctamente.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,Empleado")]
        [HttpGet("")]
        public IActionResult Index()
        {
            var citas = _citaService.ObtenerTodas();
            return View(citas);
        }

        [Authorize(Roles = "Admin,Empleado")]
        [HttpGet("revisar/{id:int}")]
        public IActionResult Revisar(int id)
        {
            var model = _citaService.ObtenerViewModel(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [Authorize(Roles = "Admin,Empleado")]
        [HttpPost("asignar/{id:int}")]
        public IActionResult Asignar(int id, int empleadoId)
        {
            var resultado = _citaService.AsignarEmpleado(id, empleadoId);
            if (!resultado.success)
                TempData["ErrorCita"] = resultado.error;

            return RedirectToAction("Revisar", new { id });
        }

        [Authorize(Roles = "Admin,Empleado")]
        [HttpPost("cambiarestado/{id:int}")]
        public IActionResult CambiarEstado(int id, string estado)
        {
            _citaService.CambiarEstado(id, estado);
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,Empleado")]
        [HttpGet("detalle/{id:int}")]
        public IActionResult Detalle(int id)
        {
            var cita = _citaService.ObtenerPorId(id);
            if (cita == null) return NotFound();
            return View(cita);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("eliminar/{id:int}")]
        public IActionResult Eliminar(int id)
        {
            _citaService.EliminarCita(id);
            return RedirectToAction("Index");
        }
    }
}