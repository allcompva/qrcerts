// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Services.OtecService
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
  public class OtecService : IOtecService
  {
    public List<Otec> GetAll() => OtecDAL.GetAll();

    public List<Otec> GetActive() => OtecDAL.GetActive();

    public Otec GetById(Guid id) => OtecDAL.GetById(id);

    public Otec GetBySlug(string slug) => OtecDAL.GetBySlug(slug);

    public bool SlugExists(string slug, Guid? excludeId = null)
    {
      return OtecDAL.SlugExists(slug, excludeId);
    }

    public int GetUserCount(Guid otecId) => OtecDAL.GetUserCount(otecId);

    public Guid Insert(Otec otec) => OtecDAL.Insert(otec);

    public void Update(Otec otec) => OtecDAL.Update(otec);

    public void Delete(Otec otec) => OtecDAL.Delete(otec);
  }
}
