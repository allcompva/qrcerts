namespace QRCerts.Api.Controllers
{
    public class GenerateFromJsonRequest
    {
        public string DocxFileName { get; set; } = ""; // nombre del archivo ya subido en uploads/docx
        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
