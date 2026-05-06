using System.Text.Json.Serialization;

namespace QRCerts.Api.Models
{
    /// <summary>
    /// Response con el PDF convertido
    /// </summary>
    public class ConvertResponse
    {
        /// <summary>
        /// Nombre del archivo PDF generado
        /// </summary>
        [JsonPropertyName("fileName")]
        public string? FileName { get; set; }

        /// <summary>
        /// Contenido del archivo PDF en formato Base64
        /// </summary>
        [JsonPropertyName("contentBase64")]
        public string ContentBase64 { get; set; } = "";
    }
}
