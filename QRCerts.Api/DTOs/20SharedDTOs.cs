// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.DTOs.BulkImportAlumnoRequest
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

#nullable enable
namespace QRCerts.Api.DTOs
{
  public class BulkImportAlumnoRequest
  {
    public string NombreApellido { get; set; } = string.Empty;

    public string RUT { get; set; } = string.Empty;

    public string Calificacion { get; set; } = string.Empty;

    public string Observaciones { get; set; } = string.Empty;

    public string certificado_otorgado { get; set; } = string.Empty;

    public string motivo_entrega { get; set; } = string.Empty;

    public int? MoodleUserId { get; set; }

    public int? MoodleCourseId { get; set; }
  }
}
