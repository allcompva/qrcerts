// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Controllers.AdminOtecUserController
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
  [Route("api/admin/otec-users")]
  [Authorize(Roles = "admin")]
  public class AdminOtecUserController : ControllerBase
  {
    private readonly IOtecUserService _otecUserService;
    private readonly IOtecService _otecService;

    public AdminOtecUserController(IOtecUserService otecUserService, IOtecService otecService)
    {
      this._otecUserService = otecUserService;
      this._otecService = otecService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      AdminOtecUserController otecUserController = this;
      List<OtecUserDetailDto> list = otecUserController._otecUserService.GetAll().Select<OtecUser, OtecUserDetailDto>((Func<OtecUser, OtecUserDetailDto>) (u =>
      {
        OtecUserDetailDto all = new OtecUserDetailDto();
        all.Id = u.Id.ToString();
        all.Username = u.Username;
        all.Email = u.Email;
        all.NombreApellido = u.NombreApellido;
        all.RUT = u.RUT;
        all.Estado = u.Estado;
        all.FechaRegistro = u.CreatedAt;
        all.LastLoginAt = u.LastLoginAt;
        OtecSummaryDto otecSummaryDto;
        if (u.Otec == null)
        {
          otecSummaryDto = (OtecSummaryDto) null;
        }
        else
        {
          otecSummaryDto = new OtecSummaryDto();
          otecSummaryDto.Id = u.Otec.Id.ToString();
          otecSummaryDto.Nombre = u.Otec.Nombre;
          otecSummaryDto.Slug = u.Otec.Slug;
        }
        all.Otec = otecSummaryDto;
        return all;
      })).ToList<OtecUserDetailDto>();
      return (IActionResult) otecUserController.Ok((object) list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
      AdminOtecUserController otecUserController = this;
      OtecUser byId = otecUserController._otecUserService.GetById(id);
      if (byId == null)
        return (IActionResult) otecUserController.NotFound((object) new
        {
          message = "OTEC user not found"
        });
      OtecUserDetailDto otecUserDetailDto1 = new OtecUserDetailDto();
      Guid id1 = byId.Id;
      otecUserDetailDto1.Id = id1.ToString();
      otecUserDetailDto1.Username = byId.Username;
      otecUserDetailDto1.Email = byId.Email;
      otecUserDetailDto1.NombreApellido = byId.NombreApellido;
      otecUserDetailDto1.RUT = byId.RUT;
      otecUserDetailDto1.Estado = byId.Estado;
      otecUserDetailDto1.FechaRegistro = byId.CreatedAt;
      otecUserDetailDto1.LastLoginAt = byId.LastLoginAt;
      OtecSummaryDto otecSummaryDto;
      if (byId.Otec == null)
      {
        otecSummaryDto = (OtecSummaryDto) null;
      }
      else
      {
        otecSummaryDto = new OtecSummaryDto();
        id1 = byId.Otec.Id;
        otecSummaryDto.Id = id1.ToString();
        otecSummaryDto.Nombre = byId.Otec.Nombre;
        otecSummaryDto.Slug = byId.Otec.Slug;
      }
      otecUserDetailDto1.Otec = otecSummaryDto;
      OtecUserDetailDto otecUserDetailDto2 = otecUserDetailDto1;
      return (IActionResult) otecUserController.Ok((object) otecUserDetailDto2);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrUpdate([FromBody] CreateOtecUserRequest request)
    {
      AdminOtecUserController otecUserController1 = this;
      if (!string.IsNullOrEmpty(request.Id))
      {
        OtecUser byId1 = otecUserController1._otecUserService.GetById(Guid.Parse(request.Id));
        if (byId1 == null)
          return (IActionResult) otecUserController1.NotFound((object) new
          {
            message = "OTEC user not found"
          });
        if (request.Username != byId1.Username && otecUserController1._otecUserService.UsernameExists(request.Username, new Guid?(Guid.Parse(request.Id))))
          return (IActionResult) otecUserController1.BadRequest((object) new
          {
            message = "Username already exists"
          });
        byId1.Username = request.Username;
        byId1.Email = request.Email ?? byId1.Email;
        byId1.NombreApellido = request.NombreApellido ?? byId1.NombreApellido;
        byId1.RUT = request.RUT ?? byId1.RUT;
        byId1.Estado = request.Estado;
        if (!string.IsNullOrEmpty(request.Password))
          byId1.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        otecUserController1._otecUserService.Update(byId1);
        OtecUser byId2 = otecUserController1._otecUserService.GetById(byId1.Id);
        AdminOtecUserController otecUserController2 = otecUserController1;
        OtecUserDetailDto otecUserDetailDto = new OtecUserDetailDto();
        otecUserDetailDto.Id = byId2.Id.ToString();
        otecUserDetailDto.Username = byId2.Username;
        otecUserDetailDto.Email = byId2.Email;
        otecUserDetailDto.NombreApellido = byId2.NombreApellido;
        otecUserDetailDto.RUT = byId2.RUT;
        otecUserDetailDto.Estado = byId2.Estado;
        otecUserDetailDto.FechaRegistro = byId2.CreatedAt;
        otecUserDetailDto.LastLoginAt = byId2.LastLoginAt;
        OtecSummaryDto otecSummaryDto;
        if (byId2.Otec == null)
        {
          otecSummaryDto = (OtecSummaryDto) null;
        }
        else
        {
          otecSummaryDto = new OtecSummaryDto();
          otecSummaryDto.Id = byId2.Otec.Id.ToString();
          otecSummaryDto.Nombre = byId2.Otec.Nombre;
          otecSummaryDto.Slug = byId2.Otec.Slug;
        }
        otecUserDetailDto.Otec = otecSummaryDto;
        return (IActionResult) otecUserController2.Ok((object) otecUserDetailDto);
      }
      if (otecUserController1._otecUserService.UsernameExists(request.Username))
        return (IActionResult) otecUserController1.BadRequest((object) new
        {
          message = "Username already exists"
        });
      Otec byId = otecUserController1._otecService.GetById(Guid.Parse(request.OtecId));
      if (byId == null)
        return (IActionResult) otecUserController1.BadRequest((object) new
        {
          message = "OTEC not found"
        });
      OtecUser user = new OtecUser()
      {
        Username = request.Username,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        Email = request.Email ?? string.Empty,
        NombreApellido = request.NombreApellido ?? string.Empty,
        RUT = request.RUT ?? string.Empty,
        OtecId = byId.Id,
        Estado = 1
      };
      user.Id = otecUserController1._otecUserService.Insert(user);
      return (IActionResult) otecUserController1.CreatedAtAction("GetById", (object) new
      {
        id = user.Id
      }, (object) new OtecUserDetailDto()
      {
        Id = user.Id.ToString(),
        Username = user.Username,
        Email = user.Email,
        NombreApellido = user.NombreApellido,
        RUT = user.RUT,
        Estado = user.Estado,
        FechaRegistro = user.CreatedAt,
        Otec = new OtecSummaryDto()
        {
          Id = byId.Id.ToString(),
          Nombre = byId.Nombre,
          Slug = byId.Slug
        }
      });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
      AdminOtecUserController otecUserController = this;
      OtecUser byId = otecUserController._otecUserService.GetById(id);
      if (byId == null)
        return (IActionResult) otecUserController.NotFound((object) new
        {
          message = "OTEC user not found"
        });
      otecUserController._otecUserService.Delete(byId);
      return (IActionResult) otecUserController.Ok((object) new
      {
        message = "OTEC user deleted successfully"
      });
    }
  }
}
