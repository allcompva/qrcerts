// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.DTOs.OtecUserDto
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

#nullable enable
namespace QRCerts.Api.DTOs
{
  public class OtecUserDto
  {
    public string Id { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? FullName { get; set; }

    public OtecSummaryDto? Otec { get; set; }
  }
}
