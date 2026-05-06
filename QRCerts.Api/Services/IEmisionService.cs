// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Services.IEmisionService
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using System;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace QRCerts.Api.Services
{
  public interface IEmisionService
  {
    Task<EmissionResult> EmitCertificatesAsync(
      Guid cursoId,
      EmitRequest request,
      CancellationToken cancellationToken = default (CancellationToken));
  }
}
