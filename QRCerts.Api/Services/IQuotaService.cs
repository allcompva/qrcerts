using QRCerts.Api.DTOs;
using QRCerts.Api.Models;

namespace QRCerts.Api.Services
{
    public interface IQuotaService
    {
        OtecQuotaConfig? GetQuotaConfig(Guid otecId);
        void SetQuotaActivo(Guid otecId, bool activo);
        OrdenCompra? GetOrdenActiva(Guid otecId);
        List<OrdenCompra> GetOrdenesByOtec(Guid otecId);
        OrdenCompra CrearOrden(Guid otecId, int cantidad, DateTime expiracion, string adminUsername, string? notas);
        List<OrdenCompraHistorial> GetHistorial(Guid otecId);

        bool PuedeEmitir(Guid otecId, int cantidad = 1);
        bool ConsumirQuota(Guid otecId, int cantidad = 1);

        QuotaStatusDto? GetQuotaStatus(Guid otecId);
    }
}
