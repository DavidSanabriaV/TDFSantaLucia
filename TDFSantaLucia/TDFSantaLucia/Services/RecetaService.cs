using System.Text;
using TDFSantaLucia.Models;
using TDFSantaLucia.Repositories;

namespace TDFSantaLucia.Services
{
    public class RecetaService : IRecetaService
    {
        private readonly IRecetaRepository _repo;
        private readonly IExpedienteRepository _expedienteRepo;
        private readonly IProductoRepository _productoRepo;

        public RecetaService(
            IRecetaRepository repo,
            IExpedienteRepository expedienteRepo,
            IProductoRepository productoRepo)
        {
            _repo = repo;
            _expedienteRepo = expedienteRepo;
            _productoRepo = productoRepo;
        }

        public List<RecetaMedica> ObtenerPorExpediente(int expedienteId)
            => _repo.ObtenerPorExpediente(expedienteId);

        public RecetaMedica? ObtenerPorId(int id)
            => _repo.ObtenerPorId(id);

        public List<Producto> ObtenerProductos()
            => _productoRepo.ObtenerTodos()
                .Where(p => p.Estado)
                .OrderBy(p => p.Nombre)
                .ToList();

        public (bool exito, string? error) Crear(RecetaMedica receta)
        {
            var expediente = _expedienteRepo.ObtenerPorId(receta.Expediente_Id);
            if (expediente == null)
                return (false, "El expediente no existe.");

            var producto = _productoRepo.ObtenerPorId(receta.Producto_Id);
            if (producto == null)
                return (false, "El producto no existe.");

            if (receta.Fecha_Vencimiento.HasValue &&
                receta.Fecha_Vencimiento.Value.Date < DateTime.Today)
            {
                return (false, "La fecha de vencimiento no puede ser anterior a hoy.");
            }

            receta.Descripcion = receta.Descripcion?.Trim();
            receta.Frecuencia = receta.Frecuencia?.Trim();
            receta.Observaciones = receta.Observaciones?.Trim();
            receta.Fecha_Emision = DateTime.Now;

            _repo.Agregar(receta);
            return (true, null);
        }

        public (bool exito, string? error) Actualizar(int id, RecetaMedica receta)
        {
            var existente = _repo.ObtenerPorId(id);
            if (existente == null)
                return (false, "La receta no existe.");

            if (receta.Fecha_Vencimiento.HasValue &&
                receta.Fecha_Vencimiento.Value.Date < DateTime.Today)
            {
                return (false, "La fecha de vencimiento no puede ser anterior a hoy.");
            }

            existente.Descripcion = receta.Descripcion?.Trim();
            existente.Frecuencia = receta.Frecuencia?.Trim();
            existente.Observaciones = receta.Observaciones?.Trim();
            existente.Producto_Id = receta.Producto_Id;
            existente.Fecha_Vencimiento = receta.Fecha_Vencimiento;

            _repo.Actualizar(existente);
            return (true, null);
        }

        public (bool exito, string? error) Eliminar(int id)
        {
            var receta = _repo.ObtenerPorId(id);
            if (receta == null)
                return (false, "La receta no existe.");

            _repo.Eliminar(id);
            return (true, null);
        }

        public byte[] GenerarPdf(RecetaMedica receta)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html><html lang='es'><head>");
            sb.AppendLine("<meta charset='UTF-8'>");
            sb.AppendLine("<style>");
            sb.AppendLine("body{font-family:Arial,sans-serif;padding:40px;color:#1F2937;}");
            sb.AppendLine(".header{display:flex;justify-content:space-between;margin-bottom:30px;}");
            sb.AppendLine(".logo{font-size:24px;font-weight:bold;color:#243C8F;}");
            sb.AppendLine(".receta-num{font-size:14px;color:#6B7280;}");
            sb.AppendLine("table{width:100%;border-collapse:collapse;margin:20px 0;}");
            sb.AppendLine("th{background:#243C8F;color:white;padding:10px;text-align:left;}");
            sb.AppendLine("td{padding:10px;border-bottom:1px solid #E5E7EB;}");
            sb.AppendLine(".footer-fac{margin-top:40px;text-align:center;color:#6B7280;font-size:12px;}");
            sb.AppendLine(".vencida{color:#B91C1C;font-weight:bold;}");
            sb.AppendLine("</style></head><body>");

            sb.AppendLine("<div class='header'>");
            sb.AppendLine("<div>");
            sb.AppendLine("<div class='logo'>Farmacia Santa Lucía</div>");
            sb.AppendLine("<div>Santa Lucía de Barva, Heredia, Costa Rica</div>");
            sb.AppendLine("<div>Tel: 2237-3040 | farmasantalucia@ice.co.cr</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div style='text-align:right;'>");
            sb.AppendLine($"<div class='receta-num'>Receta médica: <strong>#{receta.Receta_Id}</strong></div>");
            sb.AppendLine($"<div>Fecha de emisión: {receta.Fecha_Emision:dd/MM/yyyy HH:mm}</div>");
            sb.AppendLine("</div></div>");

            sb.AppendLine("<hr>");
            sb.AppendLine("<h3>Datos del Cliente</h3>");
            var cliente = receta.Expediente?.Cliente?.Usuario;
            sb.AppendLine($"<p><strong>Nombre:</strong> {cliente?.Nombre} {cliente?.Primer_Apellido} {cliente?.Segundo_Apellido}</p>");
            sb.AppendLine($"<p><strong>Cédula:</strong> {cliente?.Cedula ?? "—"}</p>");
            sb.AppendLine($"<p><strong>Correo:</strong> {cliente?.Email ?? "—"}</p>");
            sb.AppendLine($"<p><strong>Teléfono:</strong> {cliente?.Telefono ?? "—"}</p>");

            sb.AppendLine("<h3 style='margin-top:20px;'>Detalle de la Receta</h3>");
            sb.AppendLine("<table><thead><tr>");
            sb.AppendLine("<th>Medicamento</th><th>Frecuencia</th><th>Vencimiento</th>");
            sb.AppendLine("</tr></thead><tbody>");

            var nombreProducto = receta.Producto?.Nombre ?? "—";
            if (!string.IsNullOrEmpty(receta.Producto?.Marca))
                nombreProducto += $" — {receta.Producto.Marca}";

            var vencimientoTexto = "—";
            var claseVencimiento = "";
            if (receta.Fecha_Vencimiento.HasValue)
            {
                vencimientoTexto = receta.Fecha_Vencimiento.Value.ToString("dd/MM/yyyy");
                if (receta.Fecha_Vencimiento.Value.Date < DateTime.Today)
                    claseVencimiento = "vencida";
            }

            sb.AppendLine("<tr>");
            sb.AppendLine($"<td>{nombreProducto}</td>");
            sb.AppendLine($"<td>{receta.Frecuencia ?? "—"}</td>");
            sb.AppendLine($"<td class='{claseVencimiento}'>{vencimientoTexto}</td>");
            sb.AppendLine("</tr>");

            sb.AppendLine("</tbody></table>");

            if (!string.IsNullOrEmpty(receta.Descripcion))
            {
                sb.AppendLine("<h3 style='margin-top:20px;'>Indicaciones</h3>");
                sb.AppendLine($"<p>{receta.Descripcion}</p>");
            }

            if (!string.IsNullOrEmpty(receta.Observaciones))
            {
                sb.AppendLine("<h3 style='margin-top:20px;'>Observaciones</h3>");
                sb.AppendLine($"<p>{receta.Observaciones}</p>");
            }

            sb.AppendLine("<div class='footer-fac'>");
            sb.AppendLine("<p>Documento generado por Farmacia Santa Lucía.</p>");
            sb.AppendLine("<p>Esta receta debe ser presentada junto con la identificación del paciente.</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("</body></html>");

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}