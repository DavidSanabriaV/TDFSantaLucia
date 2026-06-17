using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Data;
using TDFSantaLucia.Models;
using TDFSantaLucia.Services;

namespace TDFSantaLucia.Controllers
{
    [Route("cupon")]
    public class CuponController : Controller
    {
        private readonly ICuponService _cuponService;
        private readonly UserManager<Usuario> _userManager;
        private readonly AppDbContext _db;

        public CuponController(
            ICuponService cuponService,
            UserManager<Usuario> userManager,
            AppDbContext db)
        {
            _cuponService = cuponService;
            _userManager = userManager;
            _db = db;
        }

        private async Task<Cliente?> ObtenerClienteAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return null;
            return await _db.Clientes
                .FirstOrDefaultAsync(c => c.Usuario_ID == usuario.Id);
        }

        // ── ADMIN CRUD ──────────────────────────────────────────────────────

        [HttpGet("")]
        [Authorize(Roles = "Admin,Empleado")]
        public IActionResult Index()
        {
            var cupones = _cuponService.ObtenerTodos();
            return View(cupones);
        }

        [HttpGet("detalle/{id:int}")]
        [Authorize(Roles = "Admin,Empleado")]
        public IActionResult Detalle(int id)
        {
            var cupon = _cuponService.ObtenerPorId(id);
            if (cupon == null) return NotFound();

            var yaAsignados = cupon.ClienteCupones
                .Select(cc => cc.Cliente_Id).ToHashSet();

            ViewBag.ClientesDisponibles = _db.Clientes
                .Include(c => c.Usuario)
                .Where(c => !yaAsignados.Contains(c.Cliente_Id))
                .OrderBy(c => c.Usuario!.Primer_Apellido)
                .ToList();

            return View(cupon);
        }

        [HttpGet("crear")]
        [Authorize(Roles = "Admin,Empleado")]
        public IActionResult Crear()
        {
            return View(new CuponViewModel
            {
                Fecha_Expiracion = DateTime.Today.AddMonths(1),
                Estado = true
            });
        }

        [HttpPost("crear")]
        [Authorize(Roles = "Admin,Empleado")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CuponViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = await _userManager.GetUserAsync(User);
            var (exito, error) = _cuponService.CrearCupon(model, usuario!.Id);

            if (!exito)
            {
                ModelState.AddModelError("", error!);
                return View(model);
            }

            TempData["ExitoCupon"] = "Cupón creado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet("editar/{id:int}")]
        [Authorize(Roles = "Admin,Empleado")]
        public IActionResult Editar(int id)
        {
            var cupon = _cuponService.ObtenerPorId(id);
            if (cupon == null) return NotFound();

            return View(new CuponViewModel
            {
                Cupon_Id = cupon.Cupon_Id,
                Descripcion = cupon.Descripcion!,
                Tipo_Descuento = cupon.Tipo_Descuento!,
                Valor_Descuento = cupon.Valor_Descuento,
                Fecha_Expiracion = cupon.Fecha_Expiracion,
                Estado = cupon.Estado
            });
        }

        [HttpPost("editar/{id:int}")]
        [Authorize(Roles = "Admin,Empleado")]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(CuponViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (exito, error) = _cuponService.ActualizarCupon(model);
            if (!exito)
            {
                ModelState.AddModelError("", error!);
                return View(model);
            }

            TempData["ExitoCupon"] = "Cupón actualizado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost("eliminar/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var (exito, error) = _cuponService.EliminarCupon(id);
            TempData[exito ? "ExitoCupon" : "ErrorCupon"] =
                exito ? "Cupón eliminado." : error;
            return RedirectToAction("Index");
        }

        [HttpPost("asignar")]
        [Authorize(Roles = "Admin,Empleado")]
        [ValidateAntiForgeryToken]
        public IActionResult Asignar(int cuponId, int clienteId)
        {
            var (exito, error) = _cuponService.AsignarCuponACliente(cuponId, clienteId);
            TempData[exito ? "ExitoCupon" : "ErrorCupon"] =
                exito ? "Cupón asignado correctamente." : error;
            return RedirectToAction("Detalle", new { id = cuponId });
        }

        [HttpPost("asignar-todos")]
        [Authorize(Roles = "Admin,Empleado")]
        [ValidateAntiForgeryToken]
        public IActionResult AsignarATodos(int cuponId, string? filtroRol)
        {
            var (exito, mensaje) = _cuponService.AsignarCuponATodos(cuponId, filtroRol);
            TempData[exito ? "ExitoCupon" : "ErrorCupon"] = mensaje;
            return RedirectToAction("Detalle", new { id = cuponId });
        }

        // ── CLIENTE ─────────────────────────────────────────────────────────

        [HttpGet("mis-cupones")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> MisCupones()
        {
            var cliente = await ObtenerClienteAsync();
            if (cliente == null) return RedirectToAction("Index", "Home");

            var cupones = _cuponService.ObtenerCuponesCliente(cliente.Cliente_Id);
            return View(cupones);
        }

        [HttpPost("validar")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Validar(int cuponId, string totalActual)
        {
            var cliente = await ObtenerClienteAsync();
            if (cliente == null)
                return Json(new { exito = false, mensaje = "Cliente no encontrado." });

            // Parsear con cultura invariante para que "2712.00" funcione siempre
            if (!decimal.TryParse(
                    totalActual,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal total) || total <= 0)
                return Json(new { exito = false, mensaje = "Total inválido." });

            var (exito, descuento, clienteCuponId) =
                _cuponService.ValidarYCalcularDescuento(cuponId, cliente.Cliente_Id, total);

            if (!exito)
                return Json(new { exito = false, mensaje = "Cupón no válido, ya utilizado o vencido." });

            var totalFinal = Math.Max(0, total - descuento);

            return Json(new
            {
                exito,
                descuento = descuento,
                totalFinal = totalFinal,
                clienteCuponId,
                mensaje = "Cupón aplicado correctamente."
            });
        }

        [HttpGet("disponibles")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Disponibles()
        {
            var cliente = await ObtenerClienteAsync();
            if (cliente == null) return Json(new List<object>());

            var cupones = _cuponService.ObtenerCuponesCliente(cliente.Cliente_Id)
                .Select(cc => new
                {
                    cuponId = cc.Cupon_Id,
                    descripcion = cc.Cupon!.Descripcion,
                    tipo = cc.Cupon.Tipo_Descuento,
                    valor = cc.Cupon.Valor_Descuento,
                    vence = cc.Cupon.Fecha_Expiracion.ToString("dd/MM/yyyy")
                });

            return Json(cupones);
        }
    }
}