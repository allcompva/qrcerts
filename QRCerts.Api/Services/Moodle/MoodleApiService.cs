using System.Text.Json;
using System.Web;

namespace QRCerts.Api.Services.Moodle
{
    public class MoodleApiService : IMoodleApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MoodleApiService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public MoodleApiService(HttpClient httpClient, ILogger<MoodleApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<MoodleConnectionResult> TestConnectionAsync(string moodleUrl, string token)
        {
            try
            {
                var siteInfo = await CallMoodleFunctionAsync<MoodleSiteInfo>(
                    moodleUrl, token, "core_webservice_get_site_info");

                if (siteInfo == null)
                {
                    return new MoodleConnectionResult
                    {
                        Success = false,
                        Error = "No se pudo obtener información del sitio"
                    };
                }

                return new MoodleConnectionResult
                {
                    Success = true,
                    SiteInfo = siteInfo
                };
            }
            catch (MoodleApiException ex)
            {
                _logger.LogWarning(ex, "Error de API Moodle al probar conexión");
                return new MoodleConnectionResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al probar conexión con Moodle: {Url}", moodleUrl);
                return new MoodleConnectionResult
                {
                    Success = false,
                    Error = $"Error de conexión: {ex.Message}"
                };
            }
        }

        public async Task<List<MoodleCourse>> GetCoursesAsync(string moodleUrl, string token)
        {
            try
            {
                // Primero intentamos con core_course_get_enrolled_courses_by_timeline_classification
                // que devuelve solo cursos donde el usuario está inscrito
                var courses = await CallMoodleFunctionAsync<List<MoodleCourse>>(
                    moodleUrl, token, "core_course_get_courses");

                return courses ?? new List<MoodleCourse>();
            }
            catch (MoodleApiException ex) when (ex.ErrorCode == "accessexception")
            {
                _logger.LogWarning("Sin permisos para listar todos los cursos, intentando alternativa");

                // Intentar obtener cursos del usuario actual
                try
                {
                    var result = await CallMoodleFunctionAsync<MoodleEnrolledCoursesResult>(
                        moodleUrl, token, "core_enrol_get_users_courses",
                        new Dictionary<string, string> { { "userid", "0" } }); // 0 = usuario actual

                    return result?.Courses?.Select(c => new MoodleCourse
                    {
                        Id = c.Id,
                        ShortName = c.ShortName,
                        FullName = c.FullName,
                        DisplayName = c.DisplayName
                    }).ToList() ?? new List<MoodleCourse>();
                }
                catch
                {
                    throw;
                }
            }
        }

        public async Task<List<MoodleEnrolledUser>> GetCourseStudentsAsync(
            string moodleUrl, string token, int courseId)
        {
            var users = await CallMoodleFunctionAsync<List<MoodleEnrolledUser>>(
                moodleUrl, token, "core_enrol_get_enrolled_users",
                new Dictionary<string, string> { { "courseid", courseId.ToString() } });

            // Filtrar solo estudiantes (rol student, shortname = "student")
            return users?
                .Where(u => u.Roles?.Any(r => r.ShortName == "student") ?? false)
                .ToList() ?? new List<MoodleEnrolledUser>();
        }

        public async Task<MoodleGradeReport?> GetCourseGradesAsync(
            string moodleUrl, string token, int courseId)
        {
            try
            {
                return await CallMoodleFunctionAsync<MoodleGradeReport>(
                    moodleUrl, token, "gradereport_user_get_grade_items",
                    new Dictionary<string, string> { { "courseid", courseId.ToString() } });
            }
            catch (MoodleApiException ex) when (ex.ErrorCode == "nopermissions")
            {
                _logger.LogWarning("Sin permisos para ver calificaciones del curso {CourseId}", courseId);
                return null;
            }
        }

        public async Task<List<MoodleStudentWithGrade>> GetStudentsWithGradesAsync(
            string moodleUrl, string token, int courseId, decimal notaMinAprobacion = 4.0m)
        {
            // Obtener estudiantes
            var students = await GetCourseStudentsAsync(moodleUrl, token, courseId);

            // Obtener calificaciones
            var gradeReport = await GetCourseGradesAsync(moodleUrl, token, courseId);

            var result = new List<MoodleStudentWithGrade>();

            foreach (var student in students)
            {
                var studentWithGrade = new MoodleStudentWithGrade
                {
                    User = student
                };

                // Buscar calificación del estudiante
                var userGrade = gradeReport?.UserGrades?
                    .FirstOrDefault(g => g.UserId == student.Id);

                if (userGrade?.GradeItems != null)
                {
                    // Buscar la nota final del curso (itemtype = "course")
                    var courseGrade = userGrade.GradeItems
                        .FirstOrDefault(g => g.ItemType == "course");

                    if (courseGrade != null)
                    {
                        studentWithGrade.NotaFinal = courseGrade.GradeRaw;
                        studentWithGrade.NotaFormateada = courseGrade.GradeFormatted;
                        studentWithGrade.Aprobado = courseGrade.GradeRaw >= notaMinAprobacion;
                    }
                }

                // Construir diccionario de campos disponibles
                studentWithGrade.CamposDisponibles = BuildAvailableFields(student);

                result.Add(studentWithGrade);
            }

            return result;
        }

        public async Task<List<MoodleAvailableField>> GetAvailableFieldsAsync(
            string moodleUrl, string token, int courseId)
        {
            var fields = new List<MoodleAvailableField>
            {
                // Campos estándar siempre disponibles
                new() { Key = "firstname", Label = "Nombre", Tipo = "text", EsCustom = false },
                new() { Key = "lastname", Label = "Apellido", Tipo = "text", EsCustom = false },
                new() { Key = "fullname", Label = "Nombre completo", Tipo = "text", EsCustom = false },
                new() { Key = "email", Label = "Email", Tipo = "text", EsCustom = false },
                new() { Key = "username", Label = "Usuario", Tipo = "text", EsCustom = false },
                new() { Key = "idnumber", Label = "Número de identificación", Tipo = "text", EsCustom = false },
                new() { Key = "finalgrade", Label = "Nota final", Tipo = "number", EsCustom = false }
            };

            // Obtener un estudiante de ejemplo para ver campos custom
            try
            {
                var students = await GetCourseStudentsAsync(moodleUrl, token, courseId);
                var sampleStudent = students.FirstOrDefault();

                if (sampleStudent?.CustomFields != null)
                {
                    foreach (var cf in sampleStudent.CustomFields)
                    {
                        fields.Add(new MoodleAvailableField
                        {
                            Key = $"profile_field_{cf.ShortName}",
                            Label = cf.Name,
                            Tipo = cf.Type == "text" ? "text" : cf.Type,
                            EsCustom = true,
                            ValorEjemplo = cf.Value
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error obteniendo campos custom de Moodle");
            }

            return fields;
        }

        private Dictionary<string, string> BuildAvailableFields(MoodleEnrolledUser user)
        {
            var fields = new Dictionary<string, string>
            {
                ["firstname"] = user.FirstName,
                ["lastname"] = user.LastName,
                ["fullname"] = user.FullName,
                ["email"] = user.Email,
                ["username"] = user.Username,
                ["idnumber"] = user.IdNumber
            };

            // Agregar campos custom
            if (user.CustomFields != null)
            {
                foreach (var cf in user.CustomFields)
                {
                    fields[$"profile_field_{cf.ShortName}"] = cf.Value;
                }
            }

            return fields;
        }

        private async Task<T?> CallMoodleFunctionAsync<T>(
            string moodleUrl,
            string token,
            string function,
            Dictionary<string, string>? parameters = null)
        {
            // Construir URL del web service
            var baseUrl = moodleUrl.TrimEnd('/');
            var wsUrl = $"{baseUrl}/webservice/rest/server.php";

            // Construir query string
            var queryParams = new Dictionary<string, string>
            {
                { "wstoken", token },
                { "wsfunction", function },
                { "moodlewsrestformat", "json" }
            };

            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    queryParams[p.Key] = p.Value;
                }
            }

            var queryString = string.Join("&",
                queryParams.Select(p => $"{p.Key}={HttpUtility.UrlEncode(p.Value)}"));

            var requestUrl = $"{wsUrl}?{queryString}";

            _logger.LogDebug("Llamando a Moodle: {Function}", function);

            var response = await _httpClient.GetAsync(requestUrl);
            var content = await response.Content.ReadAsStringAsync();

            // Verificar si es un error de Moodle
            if (content.Contains("\"exception\"") || content.Contains("\"errorcode\""))
            {
                var error = JsonSerializer.Deserialize<MoodleError>(content, _jsonOptions);
                throw new MoodleApiException(
                    error?.Message ?? "Error desconocido de Moodle",
                    error?.ErrorCode ?? "unknown");
            }

            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }

        // Clase auxiliar para la respuesta de cursos del usuario
        private class MoodleEnrolledCoursesResult
        {
            public List<MoodleCourse>? Courses { get; set; }
        }
    }

    public class MoodleApiException : Exception
    {
        public string ErrorCode { get; }

        public MoodleApiException(string message, string errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
