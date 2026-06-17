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

        // ── ADMIN CRUD ─────────────────────────────────────────────────────

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
                .Select(cc => cc.Cliente_Id)
                .ToHashSet();

            ViewBag.ClientesDisponibles = _db.Clientes
                .Include(c => c.Usuario)
                .Where(c => !yaAsignados.Contains(c.Cliente_Id))
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
            if (!ModelState.IsValid)
                return View(model);

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

            var model = new CuponViewModel
            {
                Cupon_Id = cupon.Cupon_Id,
                Descripcion = cupon.Descripcion!,
                Tipo_Descuento = cupon.Tipo_Descuento!,
                Valor_Descuento = cupon.Valor_Descuento,
                Fecha_Expiracion = cupon.Fecha_Expiracion,
                Estado = cupon.Estado
            };

            return View(model);
        }

        [HttpPost("editar/{id:int}")]
        [Authorize(Roles = "Admin,Empleado")]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(CuponViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

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
            if (!exito)
                TempData["ErrorCupon"] = error;
            else
                TempData["ExitoCupon"] = "Cupón eliminado correctamente.";

            return RedirectToAction("Index");
        }

        [HttpPost("asignar")]
        [Authorize(Roles = "Admin,Empleado")]
        [ValidateAntiForgeryToken]
        public IActionResult Asignar(int cuponId, int clienteId)
        {
            var (exito, error) = _cuponService.AsignarCuponACliente(cuponId, clienteId);
            if (!exito)
                TempData["ErrorCupon"] = error;
            else
                TempData["ExitoCupon"] = "Cupón asignado correctamente.";

            return RedirectToAction("Detalle", new { id = cuponId });
        }

        // ── CLIENTE: aplicar cupón desde carrito ───────────────────────────

        [HttpPost("aplicar")]
        [Authorize]
        public async Task<IActionResult> Aplicar(int cuponId)
        {
            var cliente = await ObtenerClienteAsync();
            if (cliente == null)
                return Json(new { exito = false, mensaje = "Cliente no encontrado." });

            var carritoItems = _db.CarritoItems
                .Where(c => c.Cliente_Id == cliente.Cliente_Id)
                .ToList();

            var subtotal = carritoItems.Sum(i => i.Precio * i.Cantidad);
            var impuesto = Math.Round(subtotal * 0.13m, 2);
            var total = subtotal + impuesto;

            var (exito, descuento, clienteCuponId) =
                _cuponService.AplicarCupon(cuponId, cliente.Cliente_Id, total);

            if (!exito)
                return Json(new { exito = false, mensaje = "Cupón no válido o ya utilizado." });

            var totalFinal = Math.Max(0, total - descuento);

            return Json(new
            {
                exito,
                descuento = descuento.ToString("N2"),
                totalFinal = totalFinal.ToString("N2"),
                clienteCuponId,
                mensaje = "Cupón aplicado correctamente."
            });
        }

        [HttpGet("mis-cupones")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> MisCupones()
        {
            var cliente = await ObtenerClienteAsync();
            if (cliente == null)
                return RedirectToAction("Index", "Home");

            var cupones = _cuponService.ObtenerCuponesCliente(cliente.Cliente_Id);
            return View(cupones);
        }
    }
}