// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Services.VerifyService
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using QRCerts.Api.DAL;
using QRCerts.Api.Models;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace QRCerts.Api.Services
{
  public class VerifyService : IVerifyService
  {
    private readonly IQrService _qrService;

    public VerifyService(IQrService qrService) => this._qrService = qrService;

    public async Task<VerifyResult> VerifyCertificateAsync(
      Guid idAlumno,
      Guid idCurso,
      CancellationToken cancellationToken = default (CancellationToken))
    {
      return await Task.Run<VerifyResult>((Func<Task<VerifyResult>>) (async () =>
      {
        Certificado cert = CertificadoDAL.getByGuid(idAlumno, idCurso);
        if (cert == null)
          return new VerifyResult() { Estado = "NO VÁLIDO" };
        string pdfUrl = "/" + cert.PdfFilename;
        string qrBase64 = this._qrService.GeneratePngBase64(pdfUrl);
        bool flag = await VerifyService.HeadAsync(pdfUrl);
        return new VerifyResult()
        {
          Estado = "VÁLIDO",
          OtecNombre = "OTEC",
          CursoTexto = "",
          AlumnoNombre = cert.Alumno.NombreApellido,
          RUT = cert.Alumno.RUT,
          FechaEmision = new DateTime?(cert.IssuedAt),
          Texto1 = "",
          Texto2 = "",
          PdfUrl = pdfUrl,
          PdfDisponible = flag,
          QrPdfBase64 = qrBase64
        };
      }));
    }

    private static async Task<bool> HeadAsync(string url)
    {
      try
      {
        using (HttpClient http = new HttpClient()
        {
          Timeout = TimeSpan.FromSeconds(3.0)
        })
          return (await http.SendAsync(new HttpRequestMessage(HttpMethod.Head, url))).IsSuccessStatusCode;
      }
      catch
      {
        return false;
      }
    }
  }
}
