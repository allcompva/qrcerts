namespace QRCerts.Api.Services.Moodle
{
    public interface IMoodleApiService
    {
        /// <summary>
        /// Prueba la conexión con Moodle y obtiene información del sitio.
        /// </summary>
        Task<MoodleConnectionResult> TestConnectionAsync(string moodleUrl, string token);

        /// <summary>
        /// Obtiene la lista de cursos disponibles en Moodle.
        /// </summary>
        Task<List<MoodleCourse>> GetCoursesAsync(string moodleUrl, string token);

        /// <summary>
        /// Obtiene los usuarios inscriptos en un curso específico.
        /// </summary>
        Task<List<MoodleEnrolledUser>> GetCourseStudentsAsync(string moodleUrl, string token, int courseId);

        /// <summary>
        /// Obtiene las calificaciones de los usuarios en un curso.
        /// </summary>
        Task<MoodleGradeReport?> GetCourseGradesAsync(string moodleUrl, string token, int courseId);

        /// <summary>
        /// Obtiene los estudiantes con sus calificaciones y estado de aprobación.
        /// </summary>
        Task<List<MoodleStudentWithGrade>> GetStudentsWithGradesAsync(
            string moodleUrl, string token, int courseId, decimal notaMinAprobacion = 4.0m);

        /// <summary>
        /// Obtiene los campos disponibles para mapeo basándose en los datos de los estudiantes.
        /// </summary>
        Task<List<MoodleAvailableField>> GetAvailableFieldsAsync(string moodleUrl, string token, int courseId);
    }
}
