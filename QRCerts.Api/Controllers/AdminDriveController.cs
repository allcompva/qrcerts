using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCerts.Api.Services;

namespace QRCerts.Api.Controllers
{
    [ApiController]
    [Route("api/admin/drive")]
    public class AdminDriveController : ControllerBase
    {
        private readonly IGoogleDriveService _driveService;

        public AdminDriveController(IGoogleDriveService driveService)
        {
            _driveService = driveService;
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                authenticated = _driveService.IsAuthenticated,
                message = _driveService.IsAuthenticated
                    ? "Google Drive conectado"
                    : "Google Drive no autenticado. Use POST /api/admin/drive/authenticate para conectar."
            });
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(CancellationToken ct)
        {
            try
            {
                await _driveService.AuthenticateAsync(ct);
                return Ok(new { message = "Google Drive autenticado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Error al autenticar con Google Drive",
                    error = ex.Message
                });
            }
        }
    }
}
