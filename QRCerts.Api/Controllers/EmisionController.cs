// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Controllers.EmisionController
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
  [ApiController]
  [Route("api/app/cursos/{cursoId:guid}/emision")]
  public class EmisionController : ControllerBase
  {
    private readonly IEmisionService _emisionService;

    public EmisionController(IEmisionService emisionService)
    {
      this._emisionService = emisionService;
    }

    [HttpPost]
    public async Task<IActionResult> Emit(Guid cursoId, [FromBody] EmitRequest body, CancellationToken ct)
    {
      EmisionController emisionController = this;
      EmissionResult emissionResult = await emisionController._emisionService.EmitCertificatesAsync(cursoId, body, ct);
      return emissionResult.Ok ? (IActionResult) emisionController.Ok((object) new
      {
        ok = emissionResult.Ok,
        generados = emissionResult.GeneratedFiles
      }) : (IActionResult) emisionController.BadRequest((object) new
      {
        message = emissionResult.Message
      });
    }
  }
}
