using System.Text.Json.Serialization;

namespace QRCerts.Api.Services.Moodle
{
    /// <summary>Información del sitio Moodle</summary>
    public class MoodleSiteInfo
    {
        [JsonPropertyName("sitename")]
        public string SiteName { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("firstname")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("lastname")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("userid")]
        public int UserId { get; set; }

        [JsonPropertyName("siteurl")]
        public string SiteUrl { get; set; } = string.Empty;

        [JsonPropertyName("release")]
        public string Release { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
    }

    /// <summary>Curso de Moodle</summary>
    public class MoodleCourse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("shortname")]
        public string ShortName { get; set; } = string.Empty;

        [JsonPropertyName("fullname")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("displayname")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonPropertyName("categoryid")]
        public int CategoryId { get; set; }

        [JsonPropertyName("categoryname")]
        public string? CategoryName { get; set; }

        [JsonPropertyName("visible")]
        public int Visible { get; set; }

        [JsonPropertyName("startdate")]
        public long StartDate { get; set; }

        [JsonPropertyName("enddate")]
        public long EndDate { get; set; }

        [JsonPropertyName("enrolledusercount")]
        public int? EnrolledUserCount { get; set; }
    }

    /// <summary>Usuario inscrito en un curso</summary>
    public class MoodleEnrolledUser
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("firstname")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("lastname")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("fullname")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("idnumber")]
        public string IdNumber { get; set; } = string.Empty;

        [JsonPropertyName("firstaccess")]
        public long FirstAccess { get; set; }

        [JsonPropertyName("lastaccess")]
        public long LastAccess { get; set; }

        [JsonPropertyName("suspended")]
        public bool Suspended { get; set; }

        [JsonPropertyName("customfields")]
        public List<MoodleCustomField>? CustomFields { get; set; }

        [JsonPropertyName("roles")]
        public List<MoodleRole>? Roles { get; set; }

        [JsonPropertyName("enrolledcourses")]
        public List<MoodleEnrolledCourse>? EnrolledCourses { get; set; }
    }

    /// <summary>Campo personalizado de usuario</summary>
    public class MoodleCustomField
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("shortname")]
        public string ShortName { get; set; } = string.Empty;
    }

    /// <summary>Rol del usuario</summary>
    public class MoodleRole
    {
        [JsonPropertyName("roleid")]
        public int RoleId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("shortname")]
        public string ShortName { get; set; } = string.Empty;
    }

    /// <summary>Curso en el que está inscrito el usuario</summary>
    public class MoodleEnrolledCourse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("fullname")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("shortname")]
        public string ShortName { get; set; } = string.Empty;
    }

    /// <summary>Calificación de usuario en un curso</summary>
    public class MoodleGradeReport
    {
        [JsonPropertyName("usergrades")]
        public List<MoodleUserGrade>? UserGrades { get; set; }
    }

    public class MoodleUserGrade
    {
        [JsonPropertyName("userid")]
        public int UserId { get; set; }

        [JsonPropertyName("userfullname")]
        public string UserFullName { get; set; } = string.Empty;

        [JsonPropertyName("gradeitems")]
        public List<MoodleGradeItem>? GradeItems { get; set; }
    }

    public class MoodleGradeItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("itemname")]
        public string? ItemName { get; set; }

        [JsonPropertyName("itemtype")]
        public string ItemType { get; set; } = string.Empty;

        [JsonPropertyName("graderaw")]
        public decimal? GradeRaw { get; set; }

        [JsonPropertyName("gradeformatted")]
        public string? GradeFormatted { get; set; }

        [JsonPropertyName("grademin")]
        public decimal GradeMin { get; set; }

        [JsonPropertyName("grademax")]
        public decimal GradeMax { get; set; }

        [JsonPropertyName("percentageformatted")]
        public string? PercentageFormatted { get; set; }

        [JsonPropertyName("feedback")]
        public string? Feedback { get; set; }
    }

    /// <summary>Error de Moodle API</summary>
    public class MoodleError
    {
        [JsonPropertyName("exception")]
        public string? Exception { get; set; }

        [JsonPropertyName("errorcode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("debuginfo")]
        public string? DebugInfo { get; set; }
    }

    /// <summary>Resultado de conexión a Moodle</summary>
    public class MoodleConnectionResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public MoodleSiteInfo? SiteInfo { get; set; }
    }

    /// <summary>Alumno procesado con datos de Moodle y calificación</summary>
    public class MoodleStudentWithGrade
    {
        public MoodleEnrolledUser User { get; set; } = new();
        public decimal? NotaFinal { get; set; }
        public string? NotaFormateada { get; set; }
        public bool Aprobado { get; set; }
        public Dictionary<string, string> CamposDisponibles { get; set; } = new();
    }

    /// <summary>Campos disponibles para mapeo</summary>
    public class MoodleAvailableField
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Tipo { get; set; } = "text"; // text, number, date
        public bool EsCustom { get; set; }
        public string? ValorEjemplo { get; set; }
    }
}
