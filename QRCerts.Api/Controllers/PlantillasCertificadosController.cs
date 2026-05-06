using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCerts.Api.DAL;
using QRCerts.Api.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace QRCerts.Api.Controllers
{
    [ApiController]
    [Route("api/app/plantillas-certificados")]
    [Authorize]
    public class PlantillasCertificadosController : ControllerBase
    {
        private readonly IPlantillaCertificadosService _plantillaService;

        public PlantillasCertificadosController(IPlantillaCertificadosService plantillaService)
        {
            _plantillaService = plantillaService;
        }

        [HttpGet]   
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            string? otecIdStr = User.FindFirst("otec_id")?.Value;
            if (string.IsNullOrEmpty(otecIdStr))
                return Unauthorized(new { message = "No se encontró el claim otec_id en el token." });

            Guid otecId = Guid.Parse(otecIdStr);
            var plantillas = await _plantillaService.GetByOtecAsync(otecId, ct);
            return Ok(plantillas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var plantilla = await _plantillaService.GetByIdAsync(id, ct);
            if (plantilla == null)
                return NotFound(new { message = "Plantilla no encontrada" });
            return Ok(plantilla);
        }

        public class CreatePlantillaRequest
        {
            public string Nombre { get; set; } = string.Empty;
            public string contenido_cursos { get; set; } = string.Empty;
            public string contenido_alumnos { get; set; } = string.Empty;
            public string docxPath { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePlantillaRequest request, CancellationToken ct)
        {
            try
            {
                string? otecIdStr = User.FindFirst("otec_id")?.Value;
                if (string.IsNullOrEmpty(otecIdStr))
                    return BadRequest(new { message = "OtecId not found in token" });

                Guid otecId = Guid.Parse(otecIdStr);
                Guid id = Guid.NewGuid();
                var plantilla = new Plantilla_certificados
                {
                    id = id,
                    nombre = request.Nombre,
                    contenido_cursos = request.contenido_cursos,
                    contenido_alumnos = request.contenido_alumnos,
                    path_docx = request.docxPath,
                    id_otec = otecId
                };

                await _plantillaService.CreateAsync(plantilla, ct);
                return Ok(new { message = "Plantilla creada correctamente", plantilla });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error creating plantilla", error = ex.Message });
            }
        }

        public class UpdatePlantillaRequest
        {
            public string? Nombre { get; set; }
            public string? Contenido_cursos { get; set; }
            public string? Contenido_alumnos { get; set; }
            public string? docxPath { get; set; }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePlantillaRequest request, CancellationToken ct)
        {
            try
            {
                var plantilla = await _plantillaService.GetByIdAsync(id, ct);
                if (plantilla == null)
                    return NotFound(new { message = "Plantilla no encontrada" });

                // Verificar que pertenece al otec del usuario
                string? otecIdStr = User.FindFirst("otec_id")?.Value;
                if (string.IsNullOrEmpty(otecIdStr))
                    return BadRequest(new { message = "OtecId not found in token" });

                Guid otecId = Guid.Parse(otecIdStr);
                if (plantilla.id_otec != otecId)
                    return Forbid();

                // Actualizar campos si vienen en el request
                if (request.Nombre != null)
                    plantilla.nombre = request.Nombre;
                if (request.Contenido_cursos != null)
                    plantilla.contenido_cursos = request.Contenido_cursos;
                if (request.Contenido_alumnos != null)
                    plantilla.contenido_alumnos = request.Contenido_alumnos;
                if (request.docxPath != null)
                    plantilla.path_docx = request.docxPath;

                await _plantillaService.UpdateAsync(plantilla, ct);
                return Ok(plantilla);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error updating plantilla", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                var plantilla = await _plantillaService.GetByIdAsync(id, ct);
                if (plantilla == null)
                    return NotFound(new { message = "Plantilla no encontrada" });

                // Verificar que pertenece al otec del usuario
                string? otecIdStr = User.FindFirst("otec_id")?.Value;
                if (string.IsNullOrEmpty(otecIdStr))
                    return BadRequest(new { message = "OtecId not found in token" });

                Guid otecId = Guid.Parse(otecIdStr);
                if (plantilla.id_otec != otecId)
                    return Forbid();

                bool deleted = await _plantillaService.DeleteAsync(plantilla, ct);
                if (!deleted)
                    return NotFound(new { message = "Plantilla no encontrada" });

                return Ok(new { message = "Plantilla eliminada correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error deleting plantilla", error = ex.Message });
            }
        }
    }
}
