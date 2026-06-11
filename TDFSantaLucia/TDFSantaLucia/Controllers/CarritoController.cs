using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TDFSantaLucia.Models;
using TDFSantaLucia.Services;
using TDFSantaLucia.Data;
using Microsoft.EntityFrameworkCore;

namespace TDFSantaLucia.Controllers
{
    [Route("carrito")]
    public class CarritoController : Controller
    {
        private readonly ICarritoService _carritoService;
        private readonly IProductoService _productoService;
        private readonly AppDbContext _db;

        public CarritoController(
            ICarritoService carritoService,
            IProductoService productoService,
            AppDbContext db)
        {
            _carritoService = carritoService;
            _productoService = productoService;
            _db = db;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var items = _carritoService.ObtenerCarrito();
            return View(items);
        }

        [HttpPost("agregar")]
        public IActionResult Agregar(int productoId, int cantidad = 1)
        {
            var producto = _productoService.ObtenerPorId(productoId);
            if (producto == null || !producto.Estado)
                return Json(new
                {
                    exito = false,
                    mensaje = "Producto no disponible."
                });

            _carritoService.AgregarItem(new CarritoItem
            {
                Producto_Id = producto.Producto_Id,
                Nombre = producto.Nombre,
                Imagen_URL = producto.Imagen_URL,
                Marca = producto.Marca,
                Precio = producto.Precio,
                Cantidad = cantidad,
                Receta = producto.Receta
            });

            return Json(new
            {
                exito = true,
                mensaje = $"{producto.Nombre} agregado al carrito.",
                total = _carritoService.ContarItems()
            });
        }

        [HttpPost("actualizar")]
        public IActionResult Actualizar(int productoId, int cantidad)
        {
            var stockDisponible = _db.Inventarios
                .Where(i => i.Producto_Id == productoId && i.Estado && i.Cantidad_Disponible > 0)
                .Sum(i => i.Cantidad_Disponible);

            if (cantidad > stockDisponible)
                return Json(new
                {
                    exito = false,
                    mensaje = $"Solo hay {stockDisponible} unidades disponibles.",
                    stockMax = stockDisponible
                });

            _carritoService.ActualizarCantidad(productoId, cantidad);
            var items = _carritoService.ObtenerCarrito();
            var subtotal = items.FirstOrDefault(i =>
                i.Producto_Id == productoId)?.Subtotal ?? 0;

            return Json(new
            {
                exito = true,
                subtotal = subtotal,
                total = _carritoService.ObtenerTotal(),
                count = _carritoService.ContarItems()
            });
        }

        [HttpPost("eliminar")]
        public IActionResult Eliminar(int productoId)
        {
            _carritoService.EliminarItem(productoId);
            return Json(new
            {
                exito = true,
                total = _carritoService.ObtenerTotal(),
                count = _carritoService.ContarItems()
            });
        }

        [HttpGet("contar")]
        public IActionResult Contar()
        {
            return Json(new { count = _carritoService.ContarItems() });
        }
    }
}