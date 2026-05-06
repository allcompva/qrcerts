// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Services.CursosService
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using QRCerts.Api.DAL;
using QRCerts.Api.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace QRCerts.Api.Services
{
    public class CursosService : ICursosService
    {
        public async Task<List<Curso>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Task.Run<List<Curso>>((Func<List<Curso>>)(() => CursosDAL.GetAll()));
        }
        public async Task<List<Curso>> GetByOtecAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Task.Run<List<Curso>>((Func<List<Curso>>)(() => CursosDAL.GetByOtec(id)));
        }
        public async Task<Curso?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Task.Run<Curso>((Func<Curso>)(() => CursosDAL.GetById(id)));
        }

        public async Task<Curso> CreateAsync(
          CreateCursoRequest request,
          CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Task.Run<Curso>((Func<Curso>)(() =>
            {
                Guid guid = string.IsNullOrEmpty(request.OtecId) ?
            Guid.Empty : Guid.Parse(request.OtecId);
                Guid plantillaId = string.IsNullOrEmpty(request.PlantillaId) ?
Guid.Empty : Guid.Parse(request.PlantillaId);
                DateTime venc = Convert.ToDateTime("2100/01/01");
                if (request.vencimiento != string.Empty)
                    Convert.ToDateTime(request.vencimiento);
                Curso async = new Curso()
                {
                    OtecId = guid,
                    NombreReferencia = request.NombreReferencia,
                    QrDestino = (byte)request.QrDestino,
                    FondoPath = request.FondoPath,
                    Estado = (byte)request.Estado,
                    IsFondoBloqueado = true,
                    IsLayoutBloqueado = false,
                    footer_1 = request.footer_1,
                    footer_2 = request.footer_2,
                    nombre_visualizar_certificado =
                    request.nombre_visualizar_certificado,
                    certificate_type = request.certificate_type,
                    contenidoHtml = request.contenidoHtml,
                    footerHtml = request.footerHtml,
                    LayoutJson = request.LayoutJson,
                    vencimiento = venc,
                    PlantillaId = plantillaId
                };
                CursosDAL.Insert(async);
                return async;
            }));
        }

        public async Task<Curso> UpdateAsync(
          Guid id,
          UpdateCursoRequest request,
          CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Task.Run<Curso>((Func<Curso>)(() =>
            {
                Curso byId = CursosDAL.GetById(id);
                if (byId == null)
                    throw new ArgumentException("Curso no encontrado", nameof(id));
                if (request.NombreReferencia != null)
                    byId.NombreReferencia = request.NombreReferencia;

                if (request.nombre_visualizar_certificado != null)
                    byId.nombre_visualizar_certificado = request.nombre_visualizar_certificado;

                if (request.footer_1 != null)
                    byId.footer_1 = request.footer_1;

                if (request.footer_2 != null)
                    byId.footer_2 = request.footer_2;
                if (request.vencimiento != null)
                    byId.vencimiento = Convert.ToDateTime(request.vencimiento);

                int? nullable;
                if (request.QrDestino.HasValue)
                {
                    Curso curso = byId;
                    nullable = request.QrDestino;
                    int num = (int)(byte)nullable.Value;
                    curso.QrDestino = (byte)num;
                }
                if (request.FondoPath != null)
                    byId.FondoPath = request.FondoPath;
                nullable = request.Estado;
                if (nullable.HasValue)
                {
                    Curso curso = byId;
                    nullable = request.Estado;
                    int num = (int)(byte)nullable.Value;
                    curso.Estado = (byte)num;
                }
                if (request.contenidoHtml != null)
                {
                    byId.contenidoHtml = request.contenidoHtml;
                }
                if (request.footerHtml != null)
                {
                    byId.footerHtml = request.footerHtml;
                }
                if (request.certificate_type != null)
                {
                    byId.certificate_type = request.certificate_type;
                }
                if (request.LayoutJson != null)
                {
                    byId.LayoutJson = request.LayoutJson;
                }
                CursosDAL.Update(byId);
                return byId;
            }));
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Task.Run<bool>((Func<bool>)(() =>
            {
                Curso byId = CursosDAL.GetById(id);
                if (byId == null)
                    return false;
                CursosDAL.Delete(byId);
                return true;
            }));
        }
    }
}
