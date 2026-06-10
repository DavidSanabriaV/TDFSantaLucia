using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Data;
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
        private readonly AppDbContext _db;

        public FacturaController(
            IFacturaService facturaService,
            UserManager<Usuario> userManager,
            AppDbContext db)
        {
            _facturaService = facturaService;
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

        [HttpGet("mis-facturas")]
        public async Task<IActionResult> MisFacturas(
            DateTime? desde, DateTime? hasta)
        {
            var cliente = await ObtenerClienteAsync();
            if (cliente == null)
                return RedirectToAction("Index", "Producto");

            var facturas = _facturaService
                .ObtenerPorCliente(cliente.Cliente_Id);

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
                var cliente = await ObtenerClienteAsync();
                if (cliente?.Cliente_Id != factura.Cliente_Id)
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
                var cliente = await ObtenerClienteAsync();
                if (cliente?.Cliente_Id != factura.Cliente_Id)
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