namespace QRCerts.Api.DTOs
{
    public class GenerateRequest
    {
        public string DocxFileName { get; set; } = ""; // el nombre devuelto por /api/app/upload/docx
        public string CourseId { get; set; } = "";
        public string AlumnoId { get; set; } = "";
    }
}
