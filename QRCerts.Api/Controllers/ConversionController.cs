using Microsoft.AspNetCore.Mvc;
using QRCerts.Api.Models;
using QRCerts.Api.Services;

namespace QRCerts.Api.Controllers
{
    /// <summary>
    /// Controller para conversión de documentos DOCX a PDF
    /// </summary>
    [ApiController]
    [Route("api")]
    [Produces("application/json")]
    public class ConversionController : ControllerBase
    {
        private readonly IConversionService _conversionService;
        private readonly ILogger<ConversionController> _logger;

        public ConversionController(IConversionService conversionService, ILogger<ConversionController> logger)
        {
            _conversionService = conversionService;
            _logger = logger;
        }

        /// <summary>
        /// Convierte un documento DOCX a PDF y retorna el resultado en Base64
        /// </summary>
        /// <param name="request">Documento DOCX en Base64</param>
        /// <returns>PDF convertido en Base64</returns>
        /// <response code="200">Conversión exitosa</response>
        /// <response code="400">Request inválido o Base64 malformado</response>
        /// <response code="500">Error en la conversión</response>
        [HttpPost("docx-to-pdf")]
        [ProducesResponseType(typeof(ConvertResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConvertToBase64([FromBody] ConvertRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ContentBase64))
            {
                return BadRequest(new { message = "ContentBase64 obligatorio" });
            }

            try
            {
                var response = await _conversionService.ConvertToBase64Async(request);
                return Ok(response);
            }
            catch (FormatException)
            {
                return BadRequest(new { message = "ContentBase64 no es Base64 válido." });
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Timeout en conversión");
                return StatusCode(500, new { message = "Timeout en la conversión" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en conversión");
                return StatusCode(500, new { message = "Error interno en la conversión" });
            }
        }

        /// <summary>
        /// Convierte un documento DOCX a PDF y retorna el archivo PDF directamente
        /// </summary>
        /// <param name="request">Documento DOCX en Base64</param>
        /// <returns>Archivo PDF para descarga</returns>
        /// <response code="200">Archivo PDF</response>
        /// <response code="400">Request inválido o Base64 malformado</response>
        /// <response code="500">Error en la conversión</response>
        [HttpPost("docx-to-pdf-stream")]
        [Produces("application/pdf", "application/json")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConvertToStream([FromBody] ConvertRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ContentBase64))
            {
                return BadRequest(new { message = "ContentBase64 obligatorio" });
            }

            try
            {
                var (pdfBytes, fileName) = await _conversionService.ConvertToStreamAsync(request);
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (FormatException)
            {
                return BadRequest(new { message = "ContentBase64 no es Base64 válido." });
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Timeout en conversión");
                return StatusCode(500, new { message = "Timeout en la conversión" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en conversión");
                return StatusCode(500, new { message = "Error interno en la conversión" });
            }
        }
    }
}
