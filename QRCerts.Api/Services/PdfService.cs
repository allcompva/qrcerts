// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Services.PdfService
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.IO;

#nullable enable
namespace QRCerts.Api.Services
{
  public class PdfService : IPdfService
  {
    public byte[] Render(
      byte[] fondoBytes,
      IEnumerable<(string Campo, string Texto, XRect Caja, XStringFormat Alineacion, XFont Fuente)> textos,
      byte[] qrPngBytes,
      XRect qrCaja)
    {
      using (PdfDocument pdfDocument = new PdfDocument())
      {
        PdfPage page = pdfDocument.AddPage();
        page.Size = PageSize.A4;
        page.Orientation = PageOrientation.Landscape;
        using (XGraphics xgraphics = XGraphics.FromPdfPage(page))
        {
          using (XImage image = XImage.FromStream((Func<Stream>) (() => (Stream) new MemoryStream(fondoBytes))))
            xgraphics.DrawImage(image, 0.0, 0.0, (double) page.Width, (double) page.Height);
          foreach ((string Campo, string Texto, XRect Caja, XStringFormat Alineacion, XFont Fuente) texto in textos)
            xgraphics.DrawString(texto.Texto ?? "", texto.Fuente, (XBrush) XBrushes.Black, texto.Caja, texto.Alineacion);
          using (MemoryStream ms = new MemoryStream(qrPngBytes))
          {
            using (XImage image = XImage.FromStream((Func<Stream>) (() => (Stream) ms)))
              xgraphics.DrawImage(image, qrCaja);
          }
          using (MemoryStream memoryStream = new MemoryStream())
          {
            pdfDocument.Save((Stream) memoryStream);
            return memoryStream.ToArray();
          }
        }
      }
    }
  }
}
