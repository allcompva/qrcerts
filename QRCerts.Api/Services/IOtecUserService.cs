// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Services.IOtecUserService
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using QRCerts.Api.Models;
using System;
using System.Collections.Generic;

#nullable enable
namespace QRCerts.Api.Services
{
  public interface IOtecUserService
  {
    List<OtecUser> GetAll();

    List<OtecUser> GetByOtecId(Guid otecId);

    OtecUser GetById(Guid id);

    OtecUser GetByUsername(string username);

    bool UsernameExists(string username, Guid? excludeId = null);

    Guid Insert(OtecUser user);

    void Update(OtecUser user);

    void UpdateLastLogin(Guid id);

    void Delete(OtecUser user);
  }
}
