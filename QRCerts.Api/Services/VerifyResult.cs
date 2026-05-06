// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Services.VerifyResult
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using System;

#nullable enable
namespace QRCerts.Api.Services
{
  public class VerifyResult
  {
    public string Estado { get; set; } = "NO VÁLIDO";

    public string? OtecNombre { get; set; }

    public string? CursoTexto { get; set; }

    public string? AlumnoNombre { get; set; }

    public string? RUT { get; set; }

    public DateTime? FechaEmision { get; set; }

    public string? Texto1 { get; set; }

    public string? Texto2 { get; set; }

    public string? PdfUrl { get; set; }

    public bool PdfDisponible { get; set; }

    public string? QrPdfBase64 { get; set; }
  }
}
