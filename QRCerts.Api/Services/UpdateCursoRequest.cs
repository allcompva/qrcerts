// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Services.UpdateCursoRequest
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

#nullable enable
using QRCerts.Api.Models;

namespace QRCerts.Api.Services
{
    public class UpdateCursoRequest
    {
        public string? NombreReferencia { get; set; }

        public string? BaseUrlPublica { get; set; }

        public int? QrDestino { get; set; }

        public string? FondoPath { get; set; }

        public int? Estado { get; set; }
        public string footer_1 { get; set; } = string.Empty;
        public string footer_2 { get; set; } = string.Empty;
        public string nombre_visualizar_certificado { get; set; } = string.Empty;
        public string certificate_type { get; set; } = string.Empty;
        public string contenidoHtml { get; set; } = string.Empty;
        public string footerHtml { get; set; } = string.Empty;
        public string vencimiento { get; set; } = string.Empty;
        public string? LayoutJson { get; set; }
    }
}
