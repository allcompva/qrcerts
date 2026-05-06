namespace QRCerts.Api.Models
{
    public class Curso
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OtecId { get; set; }
        public Otec? Otec { get; set; }
        public string NombreReferencia { get; set; } = string.Empty;
        public string BaseUrlPublica { get; set; } = string.Empty;
        public byte QrDestino { get; set; } = 1;
        public string FondoPath { get; set; } = string.Empty;
        public string? LayoutJson { get; set; }
        public byte Estado { get; set; } = 1;
        public bool IsFondoBloqueado { get; set; } = true;
        public bool IsBaseUrlBloqueada { get; set; } = true;
        public bool IsLayoutBloqueado { get; set; } = false;

        // Propiedades existentes (usadas por DAL)
        public string footer_1 { get; set; } = string.Empty;
        public string footer_2 { get; set; } = string.Empty;
        public string nombre_visualizar_certificado { get; set; } = string.Empty;
        public string certificate_type { get; set; } = string.Empty;
        public string contenidoHtml { get; set; } = string.Empty;
        public string footerHtml { get; set; } = string.Empty;
        public DateTime vencimiento { get; set; }

        // Alias PascalCase para compatibilidad con nuevos controllers
        public string CertificateType
        {
            get => certificate_type;
            set => certificate_type = value;
        }
        //public DateTime? Vencimiento {
        //  get => vencimiento == DateTime.MinValue ? null : vencimiento;
        //  set => vencimiento = value ?? DateTime.MinValue;
        //}

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid PlantillaId { get; set; }
    }
}
