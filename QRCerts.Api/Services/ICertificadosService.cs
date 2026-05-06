// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Services.ICertificadosService
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using QRCerts.Api.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace QRCerts.Api.Services
{
  public interface ICertificadosService
  {
    List<Certificado> read();

    Certificado getByPk(int Id);

    Certificado getByGuid(Guid AlumnoId, Guid CursoId);

    int insert(Certificado obj);

    void update(Certificado obj);

    void delete(Guid AlumnoId, Guid CursoId);

    Task<Certificado> CreateAsync(
      CreateCertificadoRequest request,
      CancellationToken cancellationToken = default (CancellationToken));

    Task<bool> DeleteAsync(Guid idAlumno, Guid idCurso, CancellationToken cancellationToken = default (CancellationToken));
  }
}
