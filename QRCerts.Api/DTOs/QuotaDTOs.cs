namespace QRCerts.Api.DTOs
{
    public class CrearOrdenRequest
    {
        public int Cantidad { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public string? Notas { get; set; }
    }

    public class SetQuotaActivoRequest
    {
        public bool QuotaActivo { get; set; }
    }

    public class OrdenCompraDto
    {
        public string Id { get; set; } = string.Empty;
        public string OtecId { get; set; } = string.Empty;
        public int CantidadComprada { get; set; }
        public int CantidadUsada { get; set; }
        public int Disponibles { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool Activa { get; set; }
        public bool Expirada { get; set; }
        public string CreadaPor { get; set; } = string.Empty;
        public string? Notas { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class QuotaStatusDto
    {
        public bool QuotaActivo { get; set; }
        public int Disponibles { get; set; }
        public int Total { get; set; }
        public DateTime? FechaExpiracion { get; set; }
        public int DiasRestantes { get; set; }
        public double PorcentajeUsado { get; set; }
        public string NivelAlerta { get; set; } = "green";
    }

    public class OrdenHistorialDto
    {
        public string Id { get; set; } = string.Empty;
        public string OrdenCompraId { get; set; } = string.Empty;
        public string Evento { get; set; } = string.Empty;
        public string? Detalle { get; set; }
        public string CreadaPor { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
