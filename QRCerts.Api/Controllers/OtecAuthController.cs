// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Controllers.OtecAuthController
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QRCerts.Api.DTOs;
using QRCerts.Api.Models;
using QRCerts.Api.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace QRCerts.Api.Controllers
{
  [ApiController]
  [Route("api/otec/auth")]
  public class OtecAuthController : ControllerBase
  {
    private readonly IOtecUserService _otecUserService;
    private readonly IOtecService _otecService;
    private readonly IConfiguration _configuration;

    public OtecAuthController(
      IOtecUserService otecUserService,
      IOtecService otecService,
      IConfiguration configuration)
    {
      this._otecUserService = otecUserService;
      this._otecService = otecService;
      this._configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] OtecLoginRequest request)
    {
        OtecAuthController otecAuthController1 = this;
      if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        return (IActionResult) otecAuthController1.BadRequest((object) new
        {
          message = "Username and password are required"
        });
      OtecUser byUsername = otecAuthController1._otecUserService.GetByUsername(request.Username);
      if (byUsername == null || !OtecAuthController.VerifyPassword(request.Password, byUsername.PasswordHash))
        return (IActionResult) otecAuthController1.Unauthorized((object) new
        {
          message = "Invalid credentials"
        });
      otecAuthController1._otecUserService.UpdateLastLogin(byUsername.Id);
      string jwtToken = otecAuthController1.GenerateJwtToken(byUsername);
      OtecAuthController otecAuthController2 = otecAuthController1;
      OtecAuthResponse otecAuthResponse = new OtecAuthResponse();
      otecAuthResponse.Token = jwtToken;
      OtecUserDto otecUserDto = new OtecUserDto();
      Guid id = byUsername.Id;
      otecUserDto.Id = id.ToString();
      otecUserDto.Username = byUsername.Username;
      otecUserDto.Role = "otec";
      otecUserDto.Email = byUsername.Email;
      otecUserDto.FullName = byUsername.NombreApellido;
      OtecSummaryDto otecSummaryDto;
      if (byUsername.Otec == null)
      {
        otecSummaryDto = (OtecSummaryDto) null;
      }
      else
      {
        otecSummaryDto = new OtecSummaryDto();
        id = byUsername.Otec.Id;
        otecSummaryDto.Id = id.ToString();
        otecSummaryDto.Nombre = byUsername.Otec.Nombre;
        otecSummaryDto.Slug = byUsername.Otec.Slug;
        otecSummaryDto.MoodleHabilitado = byUsername.Otec.MoodleHabilitado;
      }
      otecUserDto.Otec = otecSummaryDto;
      otecAuthResponse.User = otecUserDto;
      return (IActionResult) otecAuthController2.Ok((object) otecAuthResponse);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] OtecRegisterRequest request)
    {
      OtecAuthController otecAuthController = this;
      if (otecAuthController._otecUserService.UsernameExists(request.Username))
        return (IActionResult) otecAuthController.BadRequest((object) new
        {
          message = "Username already exists"
        });
      Otec byId = otecAuthController._otecService.GetById(Guid.Parse(request.OtecId));
      if (byId == null)
        return (IActionResult) otecAuthController.BadRequest((object) new
        {
          message = "OTEC not found"
        });
      OtecUser user = new OtecUser()
      {
        Username = request.Username,
        PasswordHash = OtecAuthController.HashPassword(request.Password),
        Email = request.Email ?? string.Empty,
        NombreApellido = request.FullName ?? string.Empty,
        RUT = request.RUT ?? string.Empty,
        OtecId = byId.Id
      };
      otecAuthController._otecUserService.Insert(user);
      return (IActionResult) otecAuthController.Ok((object) new
      {
        message = "OTEC user registered successfully"
      });
    }

    private string GenerateJwtToken(OtecUser otecUser)
    {
      IConfigurationSection section = this._configuration.GetSection("Jwt");
      SigningCredentials signingCredentials1 = new SigningCredentials((SecurityKey) new SymmetricSecurityKey(Encoding.UTF8.GetBytes(section["Key"] ?? "DefaultSecretKeyForDevelopment123456789")), "HS256");
      Claim[] claimArray1 = new Claim[4]
      {
        new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", otecUser.Id.ToString()),
        new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", otecUser.Username),
        new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "otec"),
        new Claim("otec_id", otecUser.OtecId.ToString())
      };
      string issuer = section["Issuer"] ?? "QRCerts";
      string audience = section["Audience"] ?? "QRCerts";
      Claim[] claimArray2 = claimArray1;
      DateTime? nullable = new DateTime?(DateTime.UtcNow.AddDays(7.0));
      SigningCredentials signingCredentials2 = signingCredentials1;
      DateTime? notBefore = new DateTime?();
      DateTime? expires = nullable;
      SigningCredentials signingCredentials3 = signingCredentials2;
      return new JwtSecurityTokenHandler().WriteToken((SecurityToken) new JwtSecurityToken(issuer, audience, (IEnumerable<Claim>) claimArray2, notBefore, expires, signingCredentials3));
    }

    private static string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    private static bool VerifyPassword(string password, string hash)
    {
      return BCrypt.Net.BCrypt.Verify(password, hash);
    }
  }
}
