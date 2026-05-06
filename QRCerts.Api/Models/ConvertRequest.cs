using System.Text.Json.Serialization;
namespace QRCerts.Api.Models
{
    /// <summary>
    /// Request para convertir un documento DOCX a PDF
    /// </summary>
    public class ConvertRequest
    {
        /// <summary>
        /// Nombre del archivo (opcional). Si no se proporciona, se usa "file.docx"
        /// </summary>
        [JsonPropertyName("fileName")]
        public string? FileName { get; set; }

        /// <summary>
        /// Contenido del archivo DOCX en formato Base64
        /// </summary>
        [JsonPropertyName("contentBase64")]
        public string ContentBase64 { get; set; } = "";
    }
}
