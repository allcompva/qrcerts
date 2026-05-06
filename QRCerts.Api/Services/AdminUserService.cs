// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Services.AdminUserService
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using QRCerts.Api.DAL;
using QRCerts.Api.Models;
using System.Collections.Generic;

#nullable enable
namespace QRCerts.Api.Services
{
  public class AdminUserService : IAdminUserService
  {
    public List<AdminUser> GetAll() => AdminUserDAL.GetAll();

    public AdminUser GetById(string id) => AdminUserDAL.GetById(id);

    public AdminUser GetByUsername(string username) => AdminUserDAL.GetByUsername(username);

    public bool UsernameExists(string username, string excludeId = null)
    {
      return AdminUserDAL.UsernameExists(username, excludeId);
    }

    public string Insert(AdminUser user) => AdminUserDAL.Insert(user);

    public void Update(AdminUser user) => AdminUserDAL.Update(user);

    public void UpdateLastLogin(string id) => AdminUserDAL.UpdateLastLogin(id);

    public void Delete(AdminUser user) => AdminUserDAL.Delete(user);
  }
}
