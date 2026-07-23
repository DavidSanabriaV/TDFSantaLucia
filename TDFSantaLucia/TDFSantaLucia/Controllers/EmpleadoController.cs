using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TDFSantaLucia.Models;
using TDFSantaLucia.Services;


namespace TDFSantaLucia.Controllers
{
    [Authorize(Roles = "Admin")]
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

        [HttpPost("eliminar/{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                await _empleadoService.EliminarEmpleadoAsync(id);
                TempData["ExitoEmpleado"] = "Empleado eliminado correctamente.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorEmpleado"] = ex.Message;
            }

            return RedirectToAction("Index");
        }
    }

}
