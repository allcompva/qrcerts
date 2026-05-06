namespace QRCerts.Api.Models
{
    public class OtecQuotaConfig
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OtecId { get; set; }
        public bool QuotaActivo { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
