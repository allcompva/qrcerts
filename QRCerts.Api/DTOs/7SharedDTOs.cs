// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.DTOs.RegistroAlumnoRequest
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using System;

#nullable enable
namespace QRCerts.Api.DTOs
{
  public class RegistroAlumnoRequest
  {
    public Guid id_alumno { get; set; } = Guid.NewGuid();

    public Guid id_curso { get; set; } = Guid.NewGuid();

    public string calificacion { get; set; } = string.Empty;

    public string observaciones { get; set; } = string.Empty;

    public string certificado_otorgado { get; set; } = string.Empty;

    public string motivo_entrega { get; set; } = string.Empty;
  }
}
