// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Services.EmisionService
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using QRCerts.Api.DAL;
using QRCerts.Api.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace QRCerts.Api.Services
{
  public class EmisionService : IEmisionService
  {
    private readonly IPdfService _pdfService;
    private readonly IQrService _qrService;
    private readonly ICertificadosService _certificadosService;

    public EmisionService(
      IPdfService pdfService,
      IQrService qrService,
      ICertificadosService certificadosService)
    {
      this._pdfService = pdfService;
      this._qrService = qrService;
      this._certificadosService = certificadosService;
    }

    public async Task<EmissionResult> EmitCertificatesAsync(
      Guid cursoId,
      EmitRequest request,
      CancellationToken cancellationToken = default (CancellationToken))
    {
      return await Task.Run<EmissionResult>((Func<EmissionResult>) (() =>
      {
        EmissionResult emissionResult = new EmissionResult();
        Curso byId = CursosDAL.GetById(cursoId);
        if (byId == null)
        {
          emissionResult.Message = "Curso no encontrado.";
          return emissionResult;
        }
        emissionResult.Message = "Función de emisión temporalmente deshabilitada - requiere actualización a REGISTRO_ALUMNOS";
        return emissionResult;
      }));
    }
  }
}
