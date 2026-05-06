namespace QRCerts.Api.Models
{
    public class OrdenCompraHistorial
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrdenCompraId { get; set; }
        public Guid OtecId { get; set; }
        public string Evento { get; set; } = string.Empty;
        public string? Detalle { get; set; }
        public string CreadaPor { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
