// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Services.OtecUserService
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using QRCerts.Api.DAL;
using QRCerts.Api.Models;
using System;
using System.Collections.Generic;

#nullable enable
namespace QRCerts.Api.Services
{
  public class OtecUserService : IOtecUserService
  {
    public List<OtecUser> GetAll() => OtecUserDAL.GetAll();

    public List<OtecUser> GetByOtecId(Guid otecId) => OtecUserDAL.GetByOtecId(otecId);

    public OtecUser GetById(Guid id) => OtecUserDAL.GetById(id);

    public OtecUser GetByUsername(string username) => OtecUserDAL.GetByUsername(username);

    public bool UsernameExists(string username, Guid? excludeId = null)
    {
      return OtecUserDAL.UsernameExists(username, excludeId);
    }

    public Guid Insert(OtecUser user) => OtecUserDAL.Insert(user);

    public void Update(OtecUser user) => OtecUserDAL.Update(user);

    public void UpdateLastLogin(Guid id) => OtecUserDAL.UpdateLastLogin(id);

    public void Delete(OtecUser user) => OtecUserDAL.Delete(user);
  }
}
