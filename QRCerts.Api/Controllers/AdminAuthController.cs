// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Controllers.AdminAuthController
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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
  [Route("api/admin/auth")]
  public class AdminAuthController : ControllerBase
  {
    private readonly IAdminUserService _adminUserService;
    private readonly IConfiguration _configuration;

    public AdminAuthController(IAdminUserService adminUserService, IConfiguration configuration)
    {
      this._adminUserService = adminUserService;
      this._configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
    {
      AdminAuthController adminAuthController = this;
      if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        return (IActionResult) adminAuthController.BadRequest((object) new
        {
          message = "Username and password are required"
        });
      AdminUser byUsername = adminAuthController._adminUserService.GetByUsername(request.Username);
      if (byUsername == null || !AdminAuthController.VerifyPassword(request.Password, byUsername.PasswordHash))
        return (IActionResult) adminAuthController.Unauthorized((object) new
        {
          message = "Invalid credentials"
        });
        adminAuthController._adminUserService.UpdateLastLogin(byUsername.Id);
      string jwtToken = adminAuthController.GenerateJwtToken(byUsername);
      return (IActionResult) adminAuthController.Ok((object) new AdminAuthResponse()
      {
        Token = jwtToken,
        User = new AdminUserDto()
        {
          Id = byUsername.Id,
          Username = byUsername.Username,
          Role = "admin",
          Email = byUsername.Email,
          FullName = byUsername.FullName
        }
      });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AdminRegisterRequest request)
    {
      AdminAuthController adminAuthController = this;
      if (adminAuthController._adminUserService.UsernameExists(request.Username))
        return (IActionResult) adminAuthController.BadRequest((object) new
        {
          message = "Username already exists"
        });
      AdminUser user = new AdminUser()
      {
        Username = request.Username,
        PasswordHash = AdminAuthController.HashPassword(request.Password),
        Email = request.Email,
        FullName = request.FullName
      };
      adminAuthController._adminUserService.Insert(user);
      return (IActionResult) adminAuthController.Ok((object) new
      {
        message = "Admin user registered successfully"
      });
    }

    [HttpPost("generate-hash")]
    public IActionResult GenerateHash([FromBody] GenerateHashRequest request)
    {
      string str = AdminAuthController.HashPassword(request.Password);
      return (IActionResult) this.Ok((object) new
      {
        password = request.Password,
        hash = str
      });
    }

    private string GenerateJwtToken(AdminUser admin)
    {
      IConfigurationSection section = this._configuration.GetSection("Jwt");
      SigningCredentials signingCredentials1 = new SigningCredentials((SecurityKey) new SymmetricSecurityKey(Encoding.UTF8.GetBytes(section["Key"] ?? "DefaultSecretKeyForDevelopment123456789")), "HS256");
      Claim[] claimArray1 = new Claim[3]
      {
        new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", admin.Id),
        new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", admin.Username),
        new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", nameof (admin))
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
