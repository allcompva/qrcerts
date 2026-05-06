using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCerts.Api.DTOs;
using QRCerts.Api.Services;

namespace QRCerts.Api.Controllers
{
    [ApiController]
    [Route("api/admin/otecs/{otecId:guid}/quota")]
    [Authorize]
    public class AdminQuotaController : ControllerBase
    {
        private readonly IQuotaService _quotaService;

        public AdminQuotaController(IQuotaService quotaService)
        {
            _quotaService = quotaService;
        }

        [HttpGet]
        public IActionResult GetConfig(Guid otecId)
        {
            var config = _quotaService.GetQuotaConfig(otecId);
            var ordenActiva = _quotaService.GetOrdenActiva(otecId);

            return Ok(new
            {
                quotaActivo = config?.QuotaActivo ?? false,
                ordenActiva = ordenActiva != null ? MapOrdenDto(ordenActiva) : null
            });
        }

        [HttpPut("toggle")]
        public IActionResult Toggle(Guid otecId, [FromBody] SetQuotaActivoRequest request)
        {
            _quotaService.SetQuotaActivo(otecId, request.QuotaActivo);
            return Ok(new { message = request.QuotaActivo ? "Quota activada" : "Quota desactivada (emisión libre)" });
        }

        [HttpPost("ordenes")]
        public IActionResult CrearOrden(Guid otecId, [FromBody] CrearOrdenRequest request)
        {
            if (request.Cantidad <= 0)
                return BadRequest(new { message = "La cantidad debe ser mayor a 0" });
            if (request.FechaExpiracion <= DateTime.UtcNow)
                return BadRequest(new { message = "La fecha de expiración debe ser futura" });

            var adminUsername = User.FindFirst("username")?.Value
                ?? User.FindFirst("sub")?.Value
                ?? "admin";

            var orden = _quotaService.CrearOrden(otecId, request.Cantidad, request.FechaExpiracion, adminUsername, request.Notas);

            return CreatedAtAction(nameof(GetOrdenes), new { otecId }, MapOrdenDto(orden));
        }

        [HttpGet("ordenes")]
        public IActionResult GetOrdenes(Guid otecId)
        {
            var ordenes = _quotaService.GetOrdenesByOtec(otecId);
            return Ok(ordenes.Select(MapOrdenDto));
        }

        [HttpGet("historial")]
        public IActionResult GetHistorial(Guid otecId)
        {
            var historial = _quotaService.GetHistorial(otecId);
            return Ok(historial.Select(h => new OrdenHistorialDto
            {
                Id = h.Id.ToString(),
                OrdenCompraId = h.OrdenCompraId.ToString(),
                Evento = h.Evento,
                Detalle = h.Detalle,
                CreadaPor = h.CreadaPor,
                CreatedAt = h.CreatedAt
            }));
        }

        private static OrdenCompraDto MapOrdenDto(Models.OrdenCompra o)
        {
            return new OrdenCompraDto
            {
                Id = o.Id.ToString(),
                OtecId = o.OtecId.ToString(),
                CantidadComprada = o.CantidadComprada,
                CantidadUsada = o.CantidadUsada,
                Disponibles = Math.Max(0, o.CantidadComprada - o.CantidadUsada),
                FechaExpiracion = o.FechaExpiracion,
                Activa = o.Activa,
                Expirada = o.FechaExpiracion <= DateTime.UtcNow,
                CreadaPor = o.CreadaPor,
                Notas = o.Notas,
                CreatedAt = o.CreatedAt
            };
        }
    }
}
