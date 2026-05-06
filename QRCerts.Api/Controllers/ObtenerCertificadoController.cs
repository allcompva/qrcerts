using Microsoft.AspNetCore.Mvc;
using QRCerts.Api.DAL;
using QRCerts.Api.Services;
using QRCerts.Api.Services.Moodle;
using System.Text.RegularExpressions;

namespace QRCerts.Api.Controllers
{
    [ApiController]
    [Route("obtener-certificado")]
    public class ObtenerCertificadoController : Controller
    {
        private readonly IMoodleApiService _moodleApiService;
        private readonly ICertificadosService _certificadosService;
        private readonly IAlumnosService _alumnosService;
        private readonly ICursosService _cursosService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ObtenerCertificadoController> _logger;

        public ObtenerCertificadoController(
            IMoodleApiService moodleApiService,
            ICertificadosService certificadosService,
            IAlumnosService alumnosService,
            ICursosService cursosService,
            IWebHostEnvironment webHostEnvironment,
            ILogger<ObtenerCertificadoController> logger)
        {
            _moodleApiService = moodleApiService;
            _certificadosService = certificadosService;
            _alumnosService = alumnosService;
            _cursosService = cursosService;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        /// <summary>
        /// Landing para obtener certificado desde enlace externo de Moodle.
        /// Recibe userid y courseid como querystring, valida y descarga el PDF directamente.
        /// Si hay error, muestra mensaje HTML simple.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int userid, [FromQuery] int courseid)
        {
            try
            {
                _logger.LogInformation("Solicitud de certificado - userid: {UserId}, courseid: {CourseId}", userid, courseid);

                // 1. Validar parámetros
                if (userid <= 0 || courseid <= 0)
                {
                    return MostrarError("Parámetros inválidos. Se requieren userid y courseid válidos.");
                }

                // 2. Buscar registro en REGISTRO_ALUMNOS por MoodleUserId y MoodleCourseId
                var registro = REGISTRO_ALUMNOS.GetByMoodleIds(userid, courseid);
                if (registro == null)
                {
                    _logger.LogWarning("No se encontró registro para userid: {UserId}, courseid: {CourseId}", userid, courseid);
                    return MostrarError("No se encontró el alumno registrado para este curso.");
                }

                // 3. Obtener el curso local para sacar el OtecId
                var cursoLocal = CursosDAL.GetById(registro.id_curso);
                if (cursoLocal == null)
                {
                    _logger.LogError("Curso local no encontrado: {CursoId}", registro.id_curso);
                    return MostrarError("El curso no existe en el sistema.");
                }

                // 4. Obtener configuración de Moodle del OTEC
                var moodleConfig = MoodleDAL.GetConfigByOtecId(cursoLocal.OtecId);
                if (moodleConfig == null || !moodleConfig.Activo)
                {
                    _logger.LogWarning("Configuración de Moodle no disponible para OtecId: {OtecId}", cursoLocal.OtecId);
                    return MostrarError("La integración con Moodle no está configurada o activa.");
                }

                // 5. Validar contra Moodle si el alumno aprobó
                var studentsWithGrades = await _moodleApiService.GetStudentsWithGradesAsync(
                    moodleConfig.MoodleUrl,
                    moodleConfig.Token,
                    courseid);

                var studentGrade = studentsWithGrades.FirstOrDefault(s => s.User.Id == userid);
                if (studentGrade == null)
                {
                    _logger.LogWarning("Alumno no encontrado en Moodle - userid: {UserId}, courseid: {CourseId}", userid, courseid);
                    return MostrarError("El alumno no está inscrito en el curso de Moodle.");
                }

                if (!studentGrade.Aprobado)
                {
                    _logger.LogInformation("Alumno no aprobó - userid: {UserId}, courseid: {CourseId}, nota: {Nota}",
                        userid, courseid, studentGrade.NotaFormateada);
                    return MostrarError("El alumno no ha aprobado el curso.");
                }

                // 6. Verificar si ya existe certificado, si no, crearlo
                var certificadoExistente = _certificadosService.getByGuid(registro.id_alumno, registro.id_curso);
                if (certificadoExistente == null)
                {
                    await _certificadosService.CreateAsync(new Services.CreateCertificadoRequest
                    {
                        AlumnoId = registro.id_alumno.ToString(),
                        CursoId = registro.id_curso.ToString()
                    }, CancellationToken.None);

                    _logger.LogInformation("Certificado creado para alumno: {AlumnoId}, curso: {CursoId}",
                        registro.id_alumno, registro.id_curso);

                    // Re-obtener el certificado recién creado
                    certificadoExistente = _certificadosService.getByGuid(registro.id_alumno, registro.id_curso);
                }

                // 7. Determinar URL de descarga
                // Re-leer por si CreateAsync actualizó el url_landing con el fileId de Drive
                certificadoExistente = _certificadosService.getByGuid(registro.id_alumno, registro.id_curso);
                string downloadUrl;

                if (certificadoExistente != null
                    && !string.IsNullOrWhiteSpace(certificadoExistente.url_landing)
                    && certificadoExistente.url_landing.Length > 10
                    && !certificadoExistente.url_landing.Contains("/"))
                {
                    // Descargar desde Drive
                    downloadUrl = $"/api/app/certificados/download/{registro.id_alumno}/{registro.id_curso}";
                }
                else
                {
                    // Fallback: generar al vuelo
                    downloadUrl = $"/api/app/upload/generate-pdf-by-ids?courseId={registro.id_curso}&alumnoId={registro.id_alumno}";
                }

                return MostrarExito(downloadUrl);
            }
            catch (MoodleApiException ex)
            {
                _logger.LogError(ex, "Error de API Moodle al validar certificado");
                return MostrarError($"Error al comunicarse con Moodle: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar solicitud de certificado");
                return MostrarError("Error interno al procesar la solicitud.");
            }
        }

        private ContentResult MostrarError(string mensaje)
        {
            var html = $@"<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Error - Certificado</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            margin: 0;
            background-color: #f5f5f5;
        }}
        .error-container {{
            background: white;
            padding: 40px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            text-align: center;
            max-width: 500px;
        }}
        .error-icon {{
            font-size: 48px;
            color: #dc3545;
            margin-bottom: 20px;
        }}
        h1 {{
            color: #333;
            font-size: 24px;
            margin-bottom: 16px;
        }}
        p {{
            color: #666;
            font-size: 16px;
            line-height: 1.5;
        }}
    </style>
</head>
<body>
    <div class=""error-container"">
        <div class=""error-icon"">&#9888;</div>
        <h1>No se puede obtener el certificado</h1>
        <p>{System.Net.WebUtility.HtmlEncode(mensaje)}</p>
    </div>
</body>
</html>";
            return Content(html, "text/html");
        }

        private ContentResult MostrarExito(string downloadUrl)
        {
            var html = $@"<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Generando Certificado</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            margin: 0;
            background-color: #f5f5f5;
        }}
        .container {{
            background: white;
            padding: 40px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            text-align: center;
            max-width: 500px;
        }}
        .icon {{
            font-size: 48px;
            margin-bottom: 20px;
        }}
        .icon-success {{
            color: darkcyan;
        }}
        .icon-error {{
            color: #dc3545;
        }}
        .spinner {{
            width: 48px;
            height: 48px;
            border: 4px solid #f3f3f3;
            border-top: 4px solid darkcyan;
            border-radius: 50%;
            animation: spin 1s linear infinite;
            margin: 0 auto 20px auto;
        }}
        @keyframes spin {{
            0% {{ transform: rotate(0deg); }}
            100% {{ transform: rotate(360deg); }}
        }}
        h1 {{
            color: #333;
            font-size: 24px;
            margin-bottom: 16px;
        }}
        p {{
            color: #666;
            font-size: 16px;
            line-height: 1.5;
        }}
        .hidden {{
            display: none;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div id=""loading"">
            <div class=""spinner""></div>
            <h1>Generando certificado...</h1>
            <p>Por favor espere, esto puede tomar unos segundos.</p>
        </div>
        <div id=""success"" class=""hidden"">
            <div class=""icon icon-success"">&#10004;</div>
            <h1>Certificado descargado con exito</h1>
            <p>Su certificado ha sido generado y descargado correctamente.</p>
        </div>
        <div id=""error"" class=""hidden"">
            <div class=""icon icon-error"">&#9888;</div>
            <h1>Error al generar certificado</h1>
            <p id=""error-message"">Ocurrio un error al generar el certificado.</p>
        </div>
    </div>
    <script>
        async function descargarCertificado() {{
            try {{
                const response = await fetch('{downloadUrl}');
                if (!response.ok) {{
                    throw new Error('Error al generar el certificado');
                }}
                const blob = await response.blob();
                const contentDisposition = response.headers.get('Content-Disposition');
                let filename = 'certificado.pdf';
                if (contentDisposition) {{
                    const match = contentDisposition.match(/filename=""?([^""]+)""?/);
                    if (match) filename = match[1];
                }}
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = filename;
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                a.remove();
                document.getElementById('loading').classList.add('hidden');
                document.getElementById('success').classList.remove('hidden');
                document.title = 'Certificado Descargado';
            }} catch (error) {{
                document.getElementById('loading').classList.add('hidden');
                document.getElementById('error').classList.remove('hidden');
                document.getElementById('error-message').textContent = error.message;
                document.title = 'Error - Certificado';
            }}
        }}
        descargarCertificado();
    </script>
</body>
</html>";
            return Content(html, "text/html");
        }

        private string GeneratePdfFileName(string cursoNombre, string alumnoRut)
        {
            string slug = GenerateSlug(!string.IsNullOrWhiteSpace(cursoNombre) ? cursoNombre : "certificado");
            string rut = !string.IsNullOrWhiteSpace(alumnoRut)
                ? alumnoRut.Replace(".", "").Replace("-", "").Replace(" ", "")
                : "sin-rut";
            return $"{slug}_{rut}.pdf";
        }

        private string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "certificado";

            string str = Regex.Replace(
                Regex.Replace(
                    Regex.Replace(text.ToLowerInvariant(), @"[^\w\s-]", " "),
                    @"\s+", "-"),
                @"-+", "-").Trim('-');

            return !string.IsNullOrWhiteSpace(str) ? str : "certificado";
        }
    }
}
