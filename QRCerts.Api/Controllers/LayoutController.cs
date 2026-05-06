// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Controllers.LayoutController
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
  [Route("api/app/cursos/{cursoId:guid}/layout")]
  public class LayoutController : ControllerBase
  {
    private readonly ILayoutService _layoutService;

    public LayoutController(ILayoutService layoutService) => this._layoutService = layoutService;

    [HttpGet]
    public async Task<IActionResult> Get(Guid cursoId, CancellationToken ct)
    {
      LayoutController layoutController = this;
      LayoutResponse layoutAsync = await layoutController._layoutService.GetLayoutAsync(cursoId, ct);
      return (IActionResult) layoutController.Ok((object) new
      {
        layoutJson = layoutAsync.LayoutJson
      });
    }

    [HttpPut]
    public async Task<IActionResult> Put(
      Guid cursoId,
      [FromBody] UpdateLayoutRequest body,
      CancellationToken ct)
    {
      LayoutController layoutController = this;
      return await layoutController._layoutService.UpdateLayoutAsync(cursoId, body, ct) ? (IActionResult) layoutController.Ok() : (IActionResult) layoutController.BadRequest((object) new
      {
        message = "El layout ya no puede modificarse tras la primera emisión."
      });
    }
  }
}
