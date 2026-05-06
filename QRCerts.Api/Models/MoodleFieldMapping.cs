namespace QRCerts.Api.Models
{
    /// <summary>
    /// Mapeo de campos Moodle a variables del certificado.
    /// Define qué campo de Moodle corresponde a cada variable del certificado.
    /// </summary>
    public class MoodleFieldMapping
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>FK a Curso local</summary>
        public Guid CursoId { get; set; }
        public Curso? Curso { get; set; }

        /// <summary>
        /// Nombre del campo en Moodle.
        /// Ejemplos: "firstname", "lastname", "email", "profile_field_rut"
        /// </summary>
        public string CampoMoodle { get; set; } = string.Empty;

        /// <summary>
        /// Variable del certificado a la que se mapea.
        /// Ejemplos: "NombreAlumno", "ApellidoAlumno", "RUT", "Email"
        /// </summary>
        public string VariableCertificado { get; set; } = string.Empty;

        /// <summary>Orden de presentación en UI</summary>
        public int Orden { get; set; }

        /// <summary>Indica si es un campo obligatorio</summary>
        public bool EsObligatorio { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Variables estándar disponibles para certificados.
    /// </summary>
    public static class VariablesCertificado
    {
        public const string NombreAlumno = "NombreAlumno";
        public const string ApellidoAlumno = "ApellidoAlumno";
        public const string NombreCompleto = "NombreCompleto";
        public const string RUT = "RUT";
        public const string Email = "Email";
        public const string NotaFinal = "NotaFinal";
        public const string FechaAprobacion = "FechaAprobacion";
        public const string NombreCurso = "NombreCurso";

        public static readonly string[] Obligatorios = { NombreCompleto, RUT };

        public static readonly Dictionary<string, string> Descripciones = new()
        {
            { NombreAlumno, "Nombre del alumno" },
            { ApellidoAlumno, "Apellido del alumno" },
            { NombreCompleto, "Nombre y apellido completo" },
            { RUT, "RUT o documento de identidad" },
            { Email, "Correo electrónico" },
            { NotaFinal, "Calificación final del curso" },
            { FechaAprobacion, "Fecha de aprobación" },
            { NombreCurso, "Nombre del curso" }
        };
    }
}
