using QRCerts.Api.DAL;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace QRCerts.Api.Services
{
    public class PlantillaCertificadosService : IPlantillaCertificadosService
    {
        public async Task<string> GetTemplateFileNameAsync(Guid courseGuid, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => Plantilla_certificados.GetTemplateFileName(courseGuid), cancellationToken);
        }
        public async Task<List<Plantilla_certificados>> GetByOtecAsync(Guid idOtec, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => Plantilla_certificados.read(idOtec), cancellationToken);
        }

        public async Task<Plantilla_certificados?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => Plantilla_certificados.getByPk(id), cancellationToken);
        }

        public async Task CreateAsync(Plantilla_certificados plantilla, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => Plantilla_certificados.insert(plantilla), cancellationToken);
        }

        public async Task UpdateAsync(Plantilla_certificados plantilla, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => Plantilla_certificados.update(plantilla), cancellationToken);
        }

        public async Task<bool> DeleteAsync(Plantilla_certificados plantilla, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                if (plantilla == null)
                    return false;
                Plantilla_certificados.delete(plantilla);
                return true;
            }, cancellationToken);
        }

        public async Task<string> GetReplacementJsonAsync(Guid courseGuid, Guid alumnoGuid, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => Plantilla_certificados.GetReplacementJson(courseGuid, alumnoGuid));
        }
    }
}
