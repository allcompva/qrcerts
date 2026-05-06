namespace QRCerts.Api.Models
{
    /// <summary>
    /// Configuración de conexión a Moodle por OTEC.
    /// Cada OTEC puede tener una única configuración de Moodle.
    /// </summary>
    public class MoodleConfig
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>FK a Otec - cada OTEC tiene una config</summary>
        public Guid OtecId { get; set; }
        public Otec? Otec { get; set; }

        /// <summary>URL base de Moodle (ej: https://moodle.ejemplo.cl)</summary>
        public string MoodleUrl { get; set; } = string.Empty;

        /// <summary>Token de Web Service generado en Moodle</summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>Indica si la configuración está activa</summary>
        public bool Activo { get; set; } = true;

        /// <summary>Última vez que se verificó la conexión exitosamente</summary>
        public DateTime? UltimaConexionExitosa { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
