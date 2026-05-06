// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Models.Otec
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using System;
using System.Collections.Generic;

#nullable enable
namespace QRCerts.Api.Models
{
  public class Otec
  {
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Nombre { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public byte Estado { get; set; } = 1;

    public bool MoodleHabilitado { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OtecUser> Users { get; set; } = (ICollection<OtecUser>) new List<OtecUser>();
  }
}
