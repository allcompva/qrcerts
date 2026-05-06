// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Services.IAlumnosService
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using QRCerts.Api.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace QRCerts.Api.Services
{
    public interface IAlumnosService
    {
        Task<bool> VerificaDuplicado(string rut, string idCurso);
        Task<Alumno?> GetByIdCertAsync(Guid idAlumno, Guid idCurso);

        Task<List<Alumno>> GetByOtecIdAsync(Guid otecId, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<Alumno>> GetByCursoIdAsync(Guid cursoId, CancellationToken cancellationToken = default(CancellationToken));

        Task<Alumno?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken));

        Task<Alumno> CreateAsync(QRCerts.Api.DTOs.CreateAlumnoRequest request, CancellationToken cancellationToken = default(CancellationToken));

        Task<Alumno> UpdateAsync(
          Guid id,
          UpdateAlumnoRequest request,
          CancellationToken cancellationToken = default(CancellationToken));

        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken));

        Task<byte[]> GenerateTemplateAsync();

        Task<PreviewResult> PreviewImportAsync(
          Guid cursoId,
          Stream fileStream,
          CancellationToken cancellationToken = default(CancellationToken));

        Task<ImportResult> CommitImportAsync(
          Guid cursoId,
          Stream fileStream,
          CancellationToken cancellationToken = default(CancellationToken));
    }
}
