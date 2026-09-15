using Microsoft.AspNetCore.Mvc;
using TDFSantaLucia.Models;
using TDFSantaLucia.Services;


namespace TDFSantaLucia.Controllers
{
    [Route("empleado")]
    public class EmpleadoController : Controller
    {
        private readonly IEmpleadoService _empleadoService;


        public EmpleadoController(IEmpleadoService empleadoService)
        {
            _empleadoService = empleadoService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var empleados = await _empleadoService.ObtenerTodosAsync();
            return View(empleados);
        }


        [HttpGet("detalle/{id:int}")]
        public async Task<IActionResult> Detalle(int id)
        {
            var model = await _empleadoService.ObtenerEmpleadoViewModelAsync(id);

            if (model == null)
                return NotFound();

            return View(model);
        }



        [HttpGet("crear")]
        public IActionResult Crear()
        {
            ViewBag.Roles = _empleadoService.ObtenerRoles();
            return View(new EmpleadoViewModel());
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear(EmpleadoViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.password))
            {
                ModelState.AddModelError(nameof(model.password), "La contraseña es obligatoria.");
            }
            if (model.ContactosEmergencia == null || !model.ContactosEmergencia.Any())
            {
                ModelState.AddModelError("", "Debe agregar al menos un contacto de emergencia.");
            }
            if (model.Alergias == null || !model.Alergias.Any())
            {
                ModelState.AddModelError("", "Debe agregar al menos una alergia (si no tiene, indique 'Ninguna').");
            }
            if (model.Enfermedades == null || !model.Enfermedades.Any())
            {
                ModelState.AddModelError("", "Debe agregar al menos una enfermedad (si no tiene, indique 'Ninguna').");
            }
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _empleadoService.ObtenerRoles();
                return View(model);
            }

            var resultado = await _empleadoService.CrearEmpleadoAsync(model);

            if (!resultado.success)
            {
                ModelState.AddModelError("", resultado.error ?? "Error al crear empleado");

                ViewBag.Roles = _empleadoService.ObtenerRoles();

                return View(model);
            }

            return RedirectToAction("Index");
        }



        [HttpGet("editar/{id:int}")]
        public async Task<IActionResult> Editar(int id)
        {
            var model = await _empleadoService.ObtenerEmpleadoViewModelAsync(id);

            if (model == null)
                return NotFound();

            ViewBag.Roles = _empleadoService.ObtenerRoles();

            return View(model);
        }

        [HttpPost("editar/{id:int}")]
        public async Task<IActionResult> Editar(EmpleadoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _empleadoService.ObtenerRoles();
                return View(model);
            }

            var resultado = await _empleadoService.ActualizarEmpleadoAsync(model);

            if (!resultado.success)
            {
                ModelState.AddModelError("", resultado.error ?? "Error al actualizar empleado");

                ViewBag.Roles = _empleadoService.ObtenerRoles();

                return View(model);
            }

            return RedirectToAction("Index");
        }

        [HttpPost("desactivar/{id:int}")]
        public async Task<IActionResult> Desactivar(int id)
        {
            try
            {
                var resultado = await _empleadoService.DesactivarEmpleadoAsync(id);

                if (resultado)
                    TempData["ExitoEmpleado"] = "Empleado desactivado correctamente.";
                else
                    TempData["ErrorEmpleado"] = "No se encontró el empleado.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorEmpleado"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost("activar/{id:int}")]
        public async Task<IActionResult> Activar(int id)
        {
            var resultado = await _empleadoService.ActivarEmpleadoAsync(id);

            if (resultado)
                TempData["ExitoEmpleado"] = "Empleado activado correctamente.";
            else
                TempData["ErrorEmpleado"] = "No se encontró el empleado.";

            return RedirectToAction("Index");
        }
    }
    }
