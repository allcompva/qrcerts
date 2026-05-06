// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Controllers.VerifyController
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using Microsoft.AspNetCore.Mvc;
using QRCerts.Api.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace QRCerts.Api.Controllers
{
  public class VerifyController : Controller
  {
    private readonly IVerifyService _verifyService;

    public VerifyController(IVerifyService verifyService) => this._verifyService = verifyService;

    [HttpGet]
    public async Task<IActionResult> Index(Guid idAlumno, Guid idCurso, CancellationToken ct)
    {
      VerifyController verifyController = this;
      VerifyResult verifyResult = await verifyController._verifyService.VerifyCertificateAsync(idAlumno, idCurso, ct);
      VerifyController.VerifyVm model = new VerifyController.VerifyVm()
      {
        Estado = verifyResult.Estado,
        OtecNombre = verifyResult.OtecNombre,
        CursoTexto = verifyResult.CursoTexto,
        AlumnoNombre = verifyResult.AlumnoNombre,
        RUT = verifyResult.RUT,
        FechaEmision = verifyResult.FechaEmision,
        Texto1 = verifyResult.Texto1,
        Texto2 = verifyResult.Texto2,
        PdfUrl = verifyResult.PdfUrl,
        PdfDisponible = verifyResult.PdfDisponible,
        QrPdfBase64 = verifyResult.QrPdfBase64
      };
      return (IActionResult) verifyController.View("Verify", (object) model);
    }

    public class VerifyVm
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
}
