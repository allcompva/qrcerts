// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Services.QrService
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using QRCoder;
using System;

#nullable enable
namespace QRCerts.Api.Services
{
  public class QrService : IQrService
  {
    public string GeneratePngBase64(string payload, int pixelsPerModule = 8)
    {
      return Convert.ToBase64String(new PngByteQRCode(new QRCodeGenerator().CreateQrCode(payload, QRCodeGenerator.ECCLevel.M)).GetGraphic(pixelsPerModule));
    }

    public byte[] GenerateQrPng(string payload, int pixelsPerModule = 8)
    {
      var qrGenerator = new QRCodeGenerator();
      var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
      var qrCode = new PngByteQRCode(qrCodeData);
      return qrCode.GetGraphic(pixelsPerModule);
    }
  }
}
