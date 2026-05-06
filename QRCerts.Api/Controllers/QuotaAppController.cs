using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCerts.Api.Services;

namespace QRCerts.Api.Controllers
{
    [ApiController]
    [Route("api/app/quota")]
    [Authorize]
    public class QuotaAppController : ControllerBase
    {
        private readonly IQuotaService _quotaService;

        public QuotaAppController(IQuotaService quotaService)
        {
            _quotaService = quotaService;
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var otecIdStr = User.FindFirst("otec_id")?.Value;
            if (string.IsNullOrEmpty(otecIdStr) || !Guid.TryParse(otecIdStr, out var otecId))
                return Unauthorized(new { message = "Invalid OTEC context" });

            var status = _quotaService.GetQuotaStatus(otecId);
            return Ok(status);
        }
    }
}
