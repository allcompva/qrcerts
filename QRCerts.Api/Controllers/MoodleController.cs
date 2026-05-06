using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCerts.Api.DAL;
using QRCerts.Api.Models;
using QRCerts.Api.Services.Moodle;

namespace QRCerts.Api.Controllers
{
    [ApiController]
    [Route("api/app/moodle")]
    [Authorize]
    public class MoodleController : ControllerBase
    {
        private readonly IMoodleApiService _moodleApi;
        private readonly ILogger<MoodleController> _logger;

        public MoodleController(
            IMoodleApiService moodleApi,
            ILogger<MoodleController> logger)
        {
            _moodleApi = moodleApi;
            _logger = logger;
        }

        private Guid? GetOtecId()
        {
            var otecIdStr = User.FindFirst("otec_id")?.Value;
            return Guid.TryParse(otecIdStr, out var otecId) ? otecId : null;
        }

        #region Configuración

        /// <summary>
        /// Obtiene la configuración de Moodle de la OTEC actual.
        /// </summary>
        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            var otecId = GetOtecId();
            if (otecId == null)
                return Unauthorized(new { message = "No se encontró el claim otec_id en el token." });

            var config = MoodleDAL.GetConfigByOtecId(otecId.Value);

            if (config == null)
                return Ok(new { configured = false });

            return Ok(new
            {
                configured = true,
                moodleUrl = config.MoodleUrl,
                activo = config.Activo,
                ultimaConexionExitosa = config.UltimaConexionExitosa,
                hasToken = !string.IsNullOrEmpty(config.Token)
            });
        }

        /// <summary>
        /// Guarda o actualiza la configuración de Moodle.
        /// </summary>
        [HttpPost("config")]
        public IActionResult SaveConfig([FromBody] SaveMoodleConfigRequest request)
        {
            var otecId = GetOtecId();
            if (otecId == null)
                return Unauthorized(new { message = "No se encontró el claim otec_id en el token." });

            var config = MoodleDAL.GetConfigByOtecId(otecId.Value);

            if (config == null)
            {
                config = new MoodleConfig
                {
                    OtecId = otecId.Value,
                    MoodleUrl = request.MoodleUrl.TrimEnd('/'),
                    Token = request.Token,
                    Activo = true
                };
            }
            else
            {
                config.MoodleUrl = request.MoodleUrl.TrimEnd('/');
                if (!string.IsNullOrEmpty(request.Token))
                {
                    config.Token = request.Token;
                }
                config.UpdatedAt = DateTime.UtcNow;
            }

            MoodleDAL.SaveConfig(config);

            return Ok(new { message = "Configuración guardada exitosamente" });
        }

        /// <summary>
        /// Prueba la conexión con Moodle.
        /// </summary>
        [HttpPost("test-connection")]
        public async Task<IActionResult> TestConnection([FromBody] TestConnectionRequest request)
        {
            var otecId = GetOtecId();
            if (otecId == null)
                return Unauthorized(new { message = "No se encontró el claim otec_id en el token." });

            string moodleUrl = request.MoodleUrl;
            string token = request.Token;

            // Si no se provee token, usar el guardado
            if (string.IsNullOrEmpty(token))
            {
                var config = MoodleDAL.GetConfigByOtecId(otecId.Value);

                if (config == null || string.IsNullOrEmpty(config.Token))
                    return BadRequest(new { message = "No hay token configurado" });

                token = config.Token;
                moodleUrl = config.MoodleUrl;
            }

            var result = await _moodleApi.TestConnectionAsync(moodleUrl.TrimEnd('/'), token);

            if (result.Success)
            {
                MoodleDAL.UpdateUltimaConexion(otecId.Value);

                return Ok(new
                {
                    success = true,
                    siteName = result.SiteInfo?.SiteName,
                    siteUrl = result.SiteInfo?.SiteUrl,
                    moodleVersion = result.SiteInfo?.Release
                });
            }

            return Ok(new
            {
                success = false,
                error = result.Error
            });
        }

        #endregion

        #region Cursos Moodle

        /// <summary>
        /// Lista los cursos disponibles en Moodle.
        /// </summary>
        [HttpGet("courses")]
        public async Task<IActionResult> GetCourses()
        {
            var otecId = GetOtecId();
            if (otecId == null)
                return Unauthorized(new { message = "No se encontró el claim otec_id en el token." });

            var config = MoodleDAL.GetConfigByOtecId(otecId.Value);

            if (config == null || !config.Activo)
                return BadRequest(new { message = "No hay configuración de Moodle activa" });

            try
            {
                var courses = await _moodleApi.GetCoursesAsync(config.MoodleUrl, config.Token);

                // Filtrar cursos visibles y no el curso "Site" (id=1)
                var filteredCourses = courses
                    .Where(c => c.Id > 1 && c.Visible == 1)
                    .Select(c => new
                    {
                        moodleId = c.Id,
                        nombre = c.FullName,
                        shortName = c.ShortName,
                        categoria = c.CategoryName ?? "",
                        cantidadAlumnos = c.EnrolledUserCount ?? 0
                    })
                    .ToList();

                return Ok(filteredCourses);
            }
            catch (MoodleApiException ex)
            {
                _logger.LogError(ex, "Error al obtener cursos de Moodle");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los estudiantes de un curso en Moodle.
        /// </summary>
        [HttpGet("courses/{moodleCourseId}/students")]
        public async Task<IActionResult> GetCourseStudents(int moodleCourseId)
        {
            var otecId = GetOtecId();
            if (otecId == null)
                return Unauthorized(new { message = "No se encontró el claim otec_id en el token." });

            var config = MoodleDAL.GetConfigByOtecId(otecId.Value);

            if (config == null || !config.Activo)
                return BadRequest(new { message = "No hay configuración de Moodle activa" });

            try
            {
                var students = await _moodleApi.GetStudentsWithGradesAsync(
                    config.MoodleUrl, config.Token, moodleCourseId);

                var result = students.Select(s => new
                {
                    moodleUserId = s.User.Id,
                    nombre = s.User.FirstName,
                    apellido = s.User.LastName,
                    nombreCompleto = s.User.FullName,
                    email = s.User.Email,
                    username = s.User.Username,
                    idNumber = s.User.IdNumber,
                    notaFinal = s.NotaFinal,
                    notaFormateada = s.NotaFormateada,
                    aprobado = s.Aprobado,
                    camposCustom = s.User.CustomFields?.Select(cf => new
                    {
                        nombre = cf.Name,
                        shortName = cf.ShortName,
                        valor = cf.Value
                    })
                }).ToList();

                return Ok(new
                {
                    total = result.Count,
                    aprobados = result.Count(s => s.aprobado),
                    estudiantes = result
                });
            }
            catch (MoodleApiException ex)
            {
                _logger.LogError(ex, "Error al obtener estudiantes del curso {CourseId}", moodleCourseId);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los campos disponibles para mapeo de un curso.
        /// </summary>
        [HttpGet("courses/{moodleCourseId}/fields")]
        public async Task<IActionResult> GetAvailableFields(int moodleCourseId)
        {
            var otecId = GetOtecId();
            if (otecId == null)
                return Unauthorized(new { message = "No se encontró el claim otec_id en el token." });

            var config = MoodleDAL.GetConfigByOtecId(otecId.Value);

            if (config == null || !config.Activo)
                return BadRequest(new { message = "No hay configuración de Moodle activa" });

            try
            {
                var fields = await _moodleApi.GetAvailableFieldsAsync(
                    config.MoodleUrl, config.Token, moodleCourseId);

                return Ok(new
                {
                    camposMoodle = fields,
                    variablesCertificado = VariablesCertificado.Descripciones
                        .Select(v => new
                        {
                            key = v.Key,
                            label = v.Value,
                            obligatorio = VariablesCertificado.Obligatorios.Contains(v.Key)
                        })
                });
            }
            catch (MoodleApiException ex)
            {
                _logger.LogError(ex, "Error al obtener campos del curso {CourseId}", moodleCourseId);
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Importación

        /// <summary>
        /// Importa estudiantes de Moodle a un curso local.
        /// </summary>
        [HttpPost("import")]
        public async Task<IActionResult> ImportStudents([FromBody] ImportStudentsRequest request)
        {
            var otecId = GetOtecId();
            if (otecId == null)
                return Unauthorized(new { message = "No se encontró el claim otec_id en el token." });

            var config = MoodleDAL.GetConfigByOtecId(otecId.Value);

            if (config == null || !config.Activo)
                return BadRequest(new { message = "No hay configuración de Moodle activa" });

            // Obtener o crear curso local
            Curso? cursoLocal = null;
            if (request.CursoLocalId.HasValue)
            {
                cursoLocal = CursosDAL.GetById(request.CursoLocalId.Value);
                if (cursoLocal == null)
                    return BadRequest(new { message = "Curso local no encontrado" });
            }
            else
            {
                // Crear nuevo curso
                cursoLocal = new Curso
                {
                    OtecId = otecId.Value,
                    NombreReferencia = request.NombreReferencia ?? request.NombreCurso ?? "Curso importado de Moodle",
                    nombre_visualizar_certificado = request.NombreCurso ?? "",
                    PlantillaId = request.PlantillaId ?? Guid.Empty,
                    vencimiento = request.FechaValidez ?? DateTime.MinValue
                };
                CursosDAL.Insert(cursoLocal);
            }

            try
            {
                // Obtener estudiantes de Moodle
                var students = await _moodleApi.GetStudentsWithGradesAsync(
                    config.MoodleUrl, config.Token, request.MoodleCourseId);

                // Filtrar solo aprobados si se indica
                if (request.SoloAprobados)
                {
                    students = students.Where(s => s.Aprobado).ToList();
                }

                int importados = 0;
                int actualizados = 0;
                int errores = 0;
                var erroresDetalle = new List<string>();

                foreach (var student in students)
                {
                    try
                    {
                        // Obtener valores según mapeo
                        var nombreCompleto = GetMappedValue(student, request.FieldMappings, "NombreCompleto")
                            ?? $"{student.User.FirstName} {student.User.LastName}";

                        var rut = GetMappedValue(student, request.FieldMappings, "RUT")
                            ?? student.User.IdNumber;

                        if (string.IsNullOrEmpty(rut))
                        {
                            errores++;
                            erroresDetalle.Add($"Alumno {nombreCompleto}: sin RUT");
                            continue;
                        }

                        // Buscar alumno existente
                        var alumnoExistente = AlumnosDAL.GetByOtecAndRut(otecId.Value, rut);

                        if (alumnoExistente != null)
                        {
                            // Actualizar nombre si cambió
                            if (alumnoExistente.NombreApellido != nombreCompleto)
                            {
                                alumnoExistente.NombreApellido = nombreCompleto;
                                AlumnosDAL.Update(alumnoExistente);
                            }
                            actualizados++;
                        }
                        else
                        {
                            // Crear nuevo alumno
                            var nuevoAlumno = new Alumno
                            {
                                OtecId = otecId.Value,
                                NombreApellido = nombreCompleto,
                                RUT = rut
                            };
                            AlumnosDAL.Insert(nuevoAlumno);
                            importados++;
                        }
                    }
                    catch (Exception ex)
                    {
                        errores++;
                        erroresDetalle.Add($"Error procesando alumno: {ex.Message}");
                    }
                }

                // Registrar el curso importado
                var cursoImportado = new MoodleCursoImportado
                {
                    OtecId = otecId.Value,
                    MoodleCourseId = request.MoodleCourseId,
                    NombreCurso = request.NombreCurso ?? "",
                    ShortName = "",
                    CursoLocalId = cursoLocal.Id,
                    CantidadAlumnos = students.Count
                };
                MoodleDAL.SaveCursoImportado(cursoImportado);

                return Ok(new
                {
                    success = true,
                    cursoLocalId = cursoLocal.Id,
                    importados,
                    actualizados,
                    errores,
                    erroresDetalle = errores > 0 ? erroresDetalle.Take(10) : null,
                    message = $"Importación completada: {importados} nuevos, {actualizados} actualizados, {errores} errores"
                });
            }
            catch (MoodleApiException ex)
            {
                _logger.LogError(ex, "Error al importar estudiantes");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el historial de cursos importados.
        /// </summary>
        [HttpGet("import/history")]
        public IActionResult GetImportHistory()
        {
            var otecId = GetOtecId();
            if (otecId == null)
                return Unauthorized(new { message = "No se encontró el claim otec_id en el token." });

            var cursos = MoodleDAL.GetCursosImportadosByOtec(otecId.Value);

            return Ok(cursos.Select(c => new
            {
                moodleCourseId = c.MoodleCourseId,
                nombreCurso = c.NombreCurso,
                cursoLocalId = c.CursoLocalId,
                cantidadAlumnos = c.CantidadAlumnos,
                ultimaSync = c.UltimaSync
            }));
        }

        private string? GetMappedValue(MoodleStudentWithGrade student, List<FieldMappingDto>? mappings, string variable)
        {
            if (mappings == null) return null;

            var mapping = mappings.FirstOrDefault(m => m.VariableCertificado == variable);
            if (mapping == null) return null;

            if (student.CamposDisponibles.TryGetValue(mapping.CampoMoodle, out var value))
            {
                return value;
            }

            return null;
        }

        #endregion
    }

    #region DTOs

    public class SaveMoodleConfigRequest
    {
        public string MoodleUrl { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class TestConnectionRequest
    {
        public string MoodleUrl { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class ImportStudentsRequest
    {
        public int MoodleCourseId { get; set; }
        public Guid? CursoLocalId { get; set; }
        public string? NombreCurso { get; set; }
        public string? NombreReferencia { get; set; }
        public Guid? PlantillaId { get; set; }
        public DateTime? FechaValidez { get; set; }
        public bool SoloAprobados { get; set; } = true;
        public List<FieldMappingDto>? FieldMappings { get; set; }
    }

    public class FieldMappingDto
    {
        public string CampoMoodle { get; set; } = string.Empty;
        public string VariableCertificado { get; set; } = string.Empty;
    }

    #endregion
}
