// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Services.PreviewResult
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using System.Collections.Generic;

#nullable enable
namespace QRCerts.Api.Services
{
  public class PreviewResult
  {
    public bool Ok { get; set; }

    public List<PreviewRow> Preview { get; set; } = new List<PreviewRow>();

    public List<string> Errores { get; set; } = new List<string>();

    public int TotalFilas { get; set; }
  }
}
