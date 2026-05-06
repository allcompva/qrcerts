// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Controllers.AlumnosImportController
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QRCerts.Api.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace QRCerts.Api.Controllers
{
  [ApiController]
  [Route("api/app/cursos/{cursoId:guid}/alumnos")]
  public class AlumnosImportController : ControllerBase
  {
    private readonly IAlumnosService _alumnosService;

    public AlumnosImportController(IAlumnosService alumnosService)
    {
      this._alumnosService = alumnosService;
    }

    [HttpGet("template")]
    public async Task<IActionResult> Template()
    {
      AlumnosImportController importController = this;
      byte[] templateAsync = await importController._alumnosService.GenerateTemplateAsync();
      return (IActionResult) importController.File(templateAsync, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Plantilla_Alumnos.xlsx");
    }

    [HttpPost("preview")]
    public async Task<IActionResult> Preview(Guid cursoId, IFormFile file, CancellationToken ct)
    {
      AlumnosImportController importController = this;
      if (file == null || file.Length == 0L)
        return (IActionResult) importController.BadRequest((object) new
        {
          message = "Sube un XLSX."
        });
      using (MemoryStream ms = new MemoryStream())
      {
        await file.CopyToAsync((Stream) ms, ct);
        ms.Position = 0L;
        PreviewResult previewResult = await importController._alumnosService.PreviewImportAsync(cursoId, (Stream) ms, ct);
        return previewResult.Ok || !previewResult.Errores.Any<string>() ? (IActionResult) importController.Ok((object) previewResult) : (IActionResult) importController.BadRequest((object) new
        {
          message = previewResult.Errores.First<string>()
        });
      }
    }

    [HttpPost("commit")]
    public async Task<IActionResult> Commit(Guid cursoId, IFormFile file, CancellationToken ct)
    {
      AlumnosImportController importController = this;
      if (file == null || file.Length == 0L)
        return (IActionResult) importController.BadRequest((object) new
        {
          message = "Sube un XLSX."
        });
      using (MemoryStream ms = new MemoryStream())
      {
        await file.CopyToAsync((Stream) ms, ct);
        ms.Position = 0L;
        ImportResult importResult = await importController._alumnosService.CommitImportAsync(cursoId, (Stream) ms, ct);
        return importResult.Ok ? (IActionResult) importController.Ok((object) new
        {
          ok = importResult.Ok,
          inserted = importResult.Inserted,
          omitted = importResult.Omitted
        }) : (IActionResult) importController.BadRequest((object) new
        {
          message = importResult.Message
        });
      }
    }
  }
}
