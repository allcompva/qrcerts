using QRCerts.Api.DAL;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace QRCerts.Api.Services
{
    public interface IPlantillaCertificadosService
    {
        Task<List<Plantilla_certificados>> GetByOtecAsync(Guid idOtec, CancellationToken cancellationToken = default);
        Task<Plantilla_certificados?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task CreateAsync(Plantilla_certificados plantilla, CancellationToken cancellationToken = default);
        Task UpdateAsync(Plantilla_certificados plantilla, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Plantilla_certificados plantilla, CancellationToken cancellationToken = default);
        Task<string> GetTemplateFileNameAsync(Guid courseGuid, CancellationToken cancellationToken = default);
        Task<string> GetReplacementJsonAsync(Guid courseGuid, Guid alumnoGuid, CancellationToken cancellationToken = default);
    }
}
