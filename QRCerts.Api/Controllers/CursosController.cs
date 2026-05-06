using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCerts.Api.Models;
using QRCerts.Api.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace QRCerts.Api.Controllers
{
  [ApiController]
  [Route("api/app/cursos")]
  [Authorize]
  public class CursosController : ControllerBase
  {
    private readonly ICursosService _cursosService;

    public CursosController(ICursosService cursosService) => this._cursosService = cursosService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
      CursosController cursosController1 = this;
      CursosController cursosController = cursosController1;
      string input = cursosController1.User.FindFirst("otec_id")?.Value;
      Guid id = Guid.Parse(input);
      return input == null ? (IActionResult) cursosController1.Unauthorized((object) "No se encontró el claim otec_id en el token.") : (IActionResult) cursosController.Ok((object) await cursosController._cursosService.GetByOtecAsync(id, ct));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
      CursosController cursosController = this;
      Curso byIdAsync = await cursosController._cursosService.GetByIdAsync(id, ct);
      IActionResult byId = byIdAsync != null ? (IActionResult) cursosController.Ok((object) byIdAsync) : (IActionResult) cursosController.NotFound((object) new
      {
        message = "Curso not found"
      });
      cursosController = (CursosController) null;
      return byId;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCursoRequest request, CancellationToken ct)
    {
      CursosController cursosController = this;
      try
      {
        string str = cursosController.User.FindFirst("otec_id")?.Value;
        if (string.IsNullOrEmpty(str))
          return (IActionResult) cursosController.BadRequest((object) new
          {
            message = "OtecId not found in token"
          });
        request.OtecId = str;
        Curso async = await cursosController._cursosService.CreateAsync(request, ct);
        return (IActionResult) cursosController.CreatedAtAction("GetById", (object) new
        {
          id = async.Id
        }, (object) async);
      }
      catch (Exception ex)
      {
        return (IActionResult) cursosController.BadRequest((object) new
        {
          message = "Error creating curso",
          error = ex.Message
        });
      }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
      Guid id,
      [FromBody] UpdateCursoRequest request,
      CancellationToken ct)
    {
      CursosController cursosController = this;
      try
      {
        return (IActionResult) cursosController.Ok((object) await cursosController._cursosService.UpdateAsync(id, request, ct));
      }
      catch (ArgumentException ex)
      {
        return (IActionResult) cursosController.NotFound((object) new
        {
          message = ex.Message
        });
      }
      catch (Exception ex)
      {
        return (IActionResult) cursosController.BadRequest((object) new
        {
          message = "Error updating curso",
          error = ex.Message
        });
      }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
      CursosController cursosController = this;
      try
      {
        return await cursosController._cursosService.DeleteAsync(id, ct) ? (IActionResult) cursosController.Ok((object) new
        {
          message = "Curso deleted successfully"
        }) : (IActionResult) cursosController.NotFound((object) new
        {
          message = "Curso not found"
        });
      }
      catch (Exception ex)
      {
        return (IActionResult) cursosController.BadRequest((object) new
        {
          message = "Error deleting curso",
          error = ex.Message
        });
      }
    }
  }
}
