namespace QRCerts.Api.Models
{
    public class OrdenCompra
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OtecId { get; set; }
        public int CantidadComprada { get; set; }
        public int CantidadUsada { get; set; } = 0;
        public DateTime FechaExpiracion { get; set; }
        public bool Activa { get; set; } = true;
        public string CreadaPor { get; set; } = string.Empty;
        public string? Notas { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
