using System.Text;
using TDFSantaLucia.Models;
using TDFSantaLucia.Repositories;

namespace TDFSantaLucia.Services
{
    public class FacturaService : IFacturaService
    {
        private readonly IFacturaRepository _repo;

        public FacturaService(IFacturaRepository repo)
        {
            _repo = repo;
        }

        public List<Factura> ObtenerTodas()
            => _repo.ObtenerTodas();

        public List<Factura> ObtenerPorCliente(int clienteId)
            => _repo.ObtenerPorCliente(clienteId);

        public Factura? ObtenerPorId(int id)
            => _repo.ObtenerPorId(id);

        public Factura? ObtenerPorPedido(int pedidoId)
            => _repo.ObtenerPorPedido(pedidoId);

        public List<Factura> FiltrarPorFecha(
            List<Factura> facturas, DateTime? desde, DateTime? hasta)
        {
            if (desde.HasValue)
                facturas = facturas
                    .Where(f => f.Fecha_Emision.Date >= desde.Value.Date)
                    .ToList();

            if (hasta.HasValue)
                facturas = facturas
                    .Where(f => f.Fecha_Emision.Date <= hasta.Value.Date)
                    .ToList();

            return facturas;
        }

        public byte[] GenerarPdf(Factura factura)
        {
            // Generamos HTML y lo convertimos a PDF con texto plano
            // Para PDF real instala QuestPDF:
            // dotnet add package QuestPDF
            // Por ahora generamos el HTML como bytes para descargar
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html><html lang='es'><head>");
            sb.AppendLine("<meta charset='UTF-8'>");
            sb.AppendLine("<style>");
            sb.AppendLine("body{font-family:Arial,sans-serif;padding:40px;color:#1F2937;}");
            sb.AppendLine(".header{display:flex;justify-content:space-between;margin-bottom:30px;}");
            sb.AppendLine(".logo{font-size:24px;font-weight:bold;color:#243C8F;}");
            sb.AppendLine(".factura-num{font-size:14px;color:#6B7280;}");
            sb.AppendLine("table{width:100%;border-collapse:collapse;margin:20px 0;}");
            sb.AppendLine("th{background:#243C8F;color:white;padding:10px;text-align:left;}");
            sb.AppendLine("td{padding:10px;border-bottom:1px solid #E5E7EB;}");
            sb.AppendLine(".totales{text-align:right;margin-top:20px;}");
            sb.AppendLine(".total-final{font-size:20px;font-weight:bold;color:#243C8F;}");
            sb.AppendLine(".footer-fac{margin-top:40px;text-align:center;color:#6B7280;font-size:12px;}");
            sb.AppendLine("</style></head><body>");

            sb.AppendLine("<div class='header'>");
            sb.AppendLine("<div>");
            sb.AppendLine("<div class='logo'>Farmacia Santa Lucía</div>");
            sb.AppendLine("<div>Santa Lucía de Barva, Heredia, Costa Rica</div>");
            sb.AppendLine("<div>Tel: 2237-3040 | farmasantalucia@ice.co.cr</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div style='text-align:right;'>");
            sb.AppendLine($"<div class='factura-num'>Factura: <strong>{factura.Numero_Factura}</strong></div>");
            sb.AppendLine($"<div>Orden: {factura.Pedido?.Numero_Orden}</div>");
            sb.AppendLine($"<div>Fecha: {factura.Fecha_Emision:dd/MM/yyyy HH:mm}</div>");
            sb.AppendLine("</div></div>");

            sb.AppendLine("<hr>");
            sb.AppendLine("<h3>Datos del Cliente</h3>");
            var cliente = factura.Cliente?.Usuario;
            sb.AppendLine($"<p><strong>Nombre:</strong> {cliente?.Nombre} {cliente?.Primer_Apellido} {cliente?.Segundo_Apellido}</p>");
            sb.AppendLine($"<p><strong>Cédula:</strong> {cliente?.Cedula ?? "—"}</p>");
            sb.AppendLine($"<p><strong>Correo:</strong> {cliente?.Email ?? "—"}</p>");
            sb.AppendLine($"<p><strong>Teléfono:</strong> {cliente?.Telefono ?? "—"}</p>");

            sb.AppendLine("<h3 style='margin-top:20px;'>Detalle de Productos</h3>");
            sb.AppendLine("<table><thead><tr>");
            sb.AppendLine("<th>Producto</th><th>Cantidad</th><th>Precio Unit.</th><th>Subtotal</th>");
            sb.AppendLine("</tr></thead><tbody>");

            foreach (var d in factura.DetallesFactura)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{d.Producto?.Nombre}</td>");
                sb.AppendLine($"<td>{d.Cantidad}</td>");
                sb.AppendLine($"<td>₡{d.Precio_Unitario:N2}</td>");
                sb.AppendLine($"<td>₡{d.Subtotal:N2}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody></table>");

            sb.AppendLine("<div class='totales'>");
            sb.AppendLine($"<p>Subtotal: ₡{factura.Subtotal:N2}</p>");
            sb.AppendLine($"<p>IVA (13%): ₡{factura.Impuesto:N2}</p>");
            if (factura.Descuento > 0)
                sb.AppendLine($"<p>Descuento: -₡{factura.Descuento:N2}</p>");
            sb.AppendLine($"<p class='total-final'>TOTAL: ₡{factura.Total:N2}</p>");
            sb.AppendLine("</div>");

            sb.AppendLine("<div class='footer-fac'>");
            sb.AppendLine("<p>¡Gracias por su compra en Farmacia Santa Lucía!</p>");
            sb.AppendLine("<p>Este documento es su comprobante de compra.</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("</body></html>");

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}