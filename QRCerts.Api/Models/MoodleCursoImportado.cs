namespace QRCerts.Api.Models
{
    /// <summary>
    /// Cache de cursos importados desde Moodle.
    /// Permite vincular un curso de Moodle con un curso local.
    /// </summary>
    public class MoodleCursoImportado
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>FK a Otec</summary>
        public Guid OtecId { get; set; }
        public Otec? Otec { get; set; }

        /// <summary>ID del curso en Moodle</summary>
        public int MoodleCourseId { get; set; }

        /// <summary>Nombre completo del curso en Moodle</summary>
        public string NombreCurso { get; set; } = string.Empty;

        /// <summary>Nombre corto del curso en Moodle</summary>
        public string ShortName { get; set; } = string.Empty;

        /// <summary>Categoría del curso en Moodle</summary>
        public string Categoria { get; set; } = string.Empty;

        /// <summary>FK opcional a Curso local (si ya está vinculado)</summary>
        public Guid? CursoLocalId { get; set; }
        public Curso? CursoLocal { get; set; }

        /// <summary>Cantidad de alumnos en el curso</summary>
        public int CantidadAlumnos { get; set; }

        /// <summary>Última sincronización con Moodle</summary>
        public DateTime UltimaSync { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
