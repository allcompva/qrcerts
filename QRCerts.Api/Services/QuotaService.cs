using QRCerts.Api.DAL;
using QRCerts.Api.DTOs;
using QRCerts.Api.Models;

namespace QRCerts.Api.Services
{
    public class QuotaService : IQuotaService
    {
        public OtecQuotaConfig? GetQuotaConfig(Guid otecId)
        {
            return QuotaDAL.GetQuotaConfig(otecId);
        }

        public void SetQuotaActivo(Guid otecId, bool activo)
        {
            QuotaDAL.SaveQuotaConfig(otecId, activo);
        }

        public OrdenCompra? GetOrdenActiva(Guid otecId)
        {
            return QuotaDAL.GetOrdenActiva(otecId);
        }

        public List<OrdenCompra> GetOrdenesByOtec(Guid otecId)
        {
            return QuotaDAL.GetOrdenesByOtec(otecId);
        }

        public OrdenCompra CrearOrden(Guid otecId, int cantidad, DateTime expiracion, string adminUsername, string? notas)
        {
            var orden = new OrdenCompra
            {
                Id = Guid.NewGuid(),
                OtecId = otecId,
                CantidadComprada = cantidad,
                FechaExpiracion = expiracion,
                CreadaPor = adminUsername,
                Notas = notas
            };
            QuotaDAL.CrearOrden(orden, adminUsername);
            return orden;
        }

        public List<OrdenCompraHistorial> GetHistorial(Guid otecId)
        {
            return QuotaDAL.GetHistorialByOtec(otecId);
        }

        public bool PuedeEmitir(Guid otecId, int cantidad = 1)
        {
            var config = QuotaDAL.GetQuotaConfig(otecId);
            if (config == null || !config.QuotaActivo)
                return true; // Quota no activa = emisión libre

            var orden = QuotaDAL.GetOrdenActiva(otecId);
            if (orden == null)
                return false; // Quota activa pero sin orden = bloqueado

            if (orden.FechaExpiracion <= DateTime.UtcNow)
                return false; // Orden vencida

            return (orden.CantidadUsada + cantidad) <= orden.CantidadComprada;
        }

        public bool ConsumirQuota(Guid otecId, int cantidad = 1)
        {
            var config = QuotaDAL.GetQuotaConfig(otecId);
            if (config == null || !config.QuotaActivo)
                return true; // Quota no activa = no consume nada

            return QuotaDAL.ConsumirQuota(otecId, cantidad);
        }

        public QuotaStatusDto? GetQuotaStatus(Guid otecId)
        {
            var config = QuotaDAL.GetQuotaConfig(otecId);
            if (config == null || !config.QuotaActivo)
                return new QuotaStatusDto { QuotaActivo = false };

            var orden = QuotaDAL.GetOrdenActiva(otecId);
            if (orden == null)
                return new QuotaStatusDto
                {
                    QuotaActivo = true,
                    Disponibles = 0,
                    Total = 0,
                    DiasRestantes = 0,
                    PorcentajeUsado = 100,
                    NivelAlerta = "red"
                };

            int disponibles = orden.CantidadComprada - orden.CantidadUsada;
            int diasRestantes = Math.Max(0, (int)(orden.FechaExpiracion - DateTime.UtcNow).TotalDays);
            double porcentajeUsado = orden.CantidadComprada > 0
                ? (double)orden.CantidadUsada / orden.CantidadComprada * 100
                : 100;

            string nivel;
            if (diasRestantes <= 3 || porcentajeUsado >= 90)
                nivel = "red";
            else if (diasRestantes < 7 || porcentajeUsado > 70)
                nivel = "yellow";
            else
                nivel = "green";

            return new QuotaStatusDto
            {
                QuotaActivo = true,
                Disponibles = Math.Max(0, disponibles),
                Total = orden.CantidadComprada,
                FechaExpiracion = orden.FechaExpiracion,
                DiasRestantes = diasRestantes,
                PorcentajeUsado = Math.Round(porcentajeUsado, 1),
                NivelAlerta = nivel
            };
        }
    }
}
