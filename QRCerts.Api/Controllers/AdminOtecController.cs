// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Controllers.AdminOtecController
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCerts.Api.DTOs;
using QRCerts.Api.Models;
using QRCerts.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable
namespace QRCerts.Api.Controllers
{
  [ApiController]
  [Route("api/admin/otecs")]
  [Authorize(Roles = "admin")]
  public class AdminOtecController : ControllerBase
  {
    private readonly IOtecService _otecService;
    private readonly IOtecUserService _otecUserService;

    public AdminOtecController(IOtecService otecService, IOtecUserService otecUserService)
    {
      this._otecService = otecService;
      this._otecUserService = otecUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      var otecs = _otecService.GetAll().Select(o => new OtecDto
      {
        Id = o.Id.ToString(),
        Nombre = o.Nombre,
        Slug = o.Slug,
        Estado = o.Estado,
        MoodleHabilitado = o.MoodleHabilitado,
        CreatedAt = o.CreatedAt,
        UserCount = _otecService.GetUserCount(o.Id)
      }).ToList();

      return Ok(otecs);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
      AdminOtecController adminOtecController = this;
      Otec byId = adminOtecController._otecService.GetById(id);
      List<OtecUser> byOtecId = adminOtecController._otecUserService.GetByOtecId(id);
      if (byId == null)
        return (IActionResult) adminOtecController.NotFound((object) new
        {
          message = "OTEC not found"
        });
      OtecDetailDto otecDetailDto = new OtecDetailDto()
      {
        Id = byId.Id.ToString(),
        Nombre = byId.Nombre,
        Slug = byId.Slug,
        Estado = byId.Estado,
        CreatedAt = byId.CreatedAt,
        Users = byOtecId.Select<OtecUser, OtecUserSummaryDto>((Func<OtecUser, OtecUserSummaryDto>) (u => new OtecUserSummaryDto()
        {
          Id = u.Id.ToString(),
          Username = u.Username,
          Email = u.Email,
          NombreApellido = u.NombreApellido,
          RUT = u.RUT,
          Estado = u.Estado
        })).ToList<OtecUserSummaryDto>()
      };
      return (IActionResult) adminOtecController.Ok((object) otecDetailDto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOtecRequest request)
    {
      AdminOtecController adminOtecController = this;
      if (adminOtecController._otecService.SlugExists(request.Slug))
        return (IActionResult) adminOtecController.BadRequest((object) new
        {
          message = "Slug already exists"
        });
      Otec otec = new Otec()
      {
        Nombre = request.Nombre,
        Slug = request.Slug,
        MoodleHabilitado = request.MoodleHabilitado
      };
      otec.Id = adminOtecController._otecService.Insert(otec);
      return (IActionResult) adminOtecController.CreatedAtAction("GetById", (object) new
      {
        id = otec.Id
      }, (object) new OtecDto()
      {
        Id = otec.Id.ToString(),
        Nombre = otec.Nombre,
        Slug = otec.Slug,
        Estado = otec.Estado,
        MoodleHabilitado = otec.MoodleHabilitado,
        CreatedAt = otec.CreatedAt,
        UserCount = 0
      });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOtecRequest request)
    {
      AdminOtecController adminOtecController = this;
      Otec byId = adminOtecController._otecService.GetById(id);
      if (byId == null)
        return (IActionResult) adminOtecController.NotFound((object) new
        {
          message = "OTEC not found"
        });
      if (request.Slug != byId.Slug && adminOtecController._otecService.SlugExists(request.Slug, new Guid?(id)))
        return (IActionResult) adminOtecController.BadRequest((object) new
        {
          message = "Slug already exists"
        });
      byId.Nombre = request.Nombre;
      byId.Slug = request.Slug;
      byId.Estado = request.Estado;
      byId.MoodleHabilitado = request.MoodleHabilitado;
      adminOtecController._otecService.Update(byId);
      return (IActionResult) adminOtecController.Ok((object) new OtecDto()
      {
        Id = byId.Id.ToString(),
        Nombre = byId.Nombre,
        Slug = byId.Slug,
        Estado = byId.Estado,
        MoodleHabilitado = byId.MoodleHabilitado,
        CreatedAt = byId.CreatedAt,
        UserCount = adminOtecController._otecService.GetUserCount(byId.Id)
      });
    }

    [HttpGet("{id}/users")]
    public async Task<IActionResult> GetUsersByOtecId(Guid id)
    {
      AdminOtecController adminOtecController = this;
      if (adminOtecController._otecService.GetById(id) == null)
        return (IActionResult) adminOtecController.NotFound((object) new
        {
          message = "OTEC not found"
        });
      List<OtecUser> list = adminOtecController._otecUserService.GetByOtecId(id).Select<OtecUser, OtecUser>((Func<OtecUser, OtecUser>) (u => new OtecUser()
      {
        Id = u.Id,
        Username = u.Username,
        Email = u.Email,
        NombreApellido = u.NombreApellido,
        RUT = u.RUT,
        Estado = u.Estado,
        OtecId = u.OtecId
      })).ToList<OtecUser>();
      return (IActionResult) adminOtecController.Ok((object) list);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
      AdminOtecController adminOtecController = this;
      Otec byId = adminOtecController._otecService.GetById(id);
      int userCount = adminOtecController._otecService.GetUserCount(id);
      if (byId == null)
        return (IActionResult) adminOtecController.NotFound((object) new
        {
          message = "OTEC not found"
        });
      if (userCount > 0)
        return (IActionResult) adminOtecController.BadRequest((object) new
        {
          message = "Cannot delete OTEC with associated users. Please remove all users first."
        });
      adminOtecController._otecService.Delete(byId);
      return (IActionResult) adminOtecController.Ok((object) new
      {
        message = "OTEC deleted successfully"
      });
    }
  }
}
