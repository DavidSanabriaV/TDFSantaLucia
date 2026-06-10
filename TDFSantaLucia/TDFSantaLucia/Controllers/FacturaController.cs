using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TDFSantaLucia.Models;
using TDFSantaLucia.Services;

namespace TDFSantaLucia.Controllers
{
    [Authorize]
    [Route("factura")]
    public class FacturaController : Controller
    {
        private readonly IFacturaService _facturaService;
        private readonly UserManager<Usuario> _userManager;

        public FacturaController(
            IFacturaService facturaService,
            UserManager<Usuario> userManager)
        {
            _facturaService = facturaService;
            _userManager = userManager;
        }

        [HttpGet("mis-facturas")]
        public async Task<IActionResult> MisFacturas(
            DateTime? desde, DateTime? hasta)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario?.Cliente == null)
                return RedirectToAction("Index", "Producto");

            var facturas = _facturaService
                .ObtenerPorCliente(usuario.Cliente.Cliente_Id);

            facturas = _facturaService.FiltrarPorFecha(facturas, desde, hasta);

            ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");

            return View(facturas);
        }

        [HttpGet("detalle/{id:int}")]
        public async Task<IActionResult> Detalle(int id)
        {
            var factura = _facturaService.ObtenerPorId(id);
            if (factura == null) return NotFound();

            if (!User.IsInRole("Admin") && !User.IsInRole("Empleado"))
            {
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario?.Cliente?.Cliente_Id != factura.Cliente_Id)
                    return Forbid();
            }

            return View(factura);
        }

        [HttpGet("descargar/{id:int}")]
        public async Task<IActionResult> Descargar(int id)
        {
            var factura = _facturaService.ObtenerPorId(id);
            if (factura == null) return NotFound();

            if (!User.IsInRole("Admin") && !User.IsInRole("Empleado"))
            {
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario?.Cliente?.Cliente_Id != factura.Cliente_Id)
                    return Forbid();
            }

            var bytes = _facturaService.GenerarPdf(factura);
            return File(bytes, "text/html",
                $"Factura-{factura.Numero_Factura}.html");
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin,Empleado")]
        public IActionResult Admin(DateTime? desde, DateTime? hasta)
        {
            var facturas = _facturaService.ObtenerTodas();
            facturas = _facturaService.FiltrarPorFecha(facturas, desde, hasta);

            ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");

            return View(facturas);
        }
    }
}