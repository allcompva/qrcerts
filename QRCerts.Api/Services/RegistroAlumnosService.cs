// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Services.RegistroAlumnosService
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using QRCerts.Api.DAL;
using System;

#nullable enable
namespace QRCerts.Api.Services
{
  public class RegistroAlumnosService : IRegistroAlumnosService
  {
    public void Insert(REGISTRO_ALUMNOS obj) => REGISTRO_ALUMNOS.Insert(obj);

    public void delete(Guid id_alumno, Guid id_curso)
    {
      REGISTRO_ALUMNOS.delete(id_alumno, id_curso);
    }
  }
}
