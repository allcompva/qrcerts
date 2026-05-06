// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Models.EmisionLote
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using System;

#nullable enable
namespace QRCerts.Api.Models
{
  public class EmisionLote
  {
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CursoId { get; set; }

    public Curso? Curso { get; set; }

    public int Total { get; set; }

    public int Generados { get; set; }

    public byte Estado { get; set; } = 1;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? FinishedAt { get; set; }

    public string? Log { get; set; }
  }
}
