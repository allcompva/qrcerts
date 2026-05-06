// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Controllers.PublicController
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using Microsoft.AspNetCore.Mvc;
using QRCerts.Api.DTOs;
using QRCerts.Api.Models;
using QRCerts.Api.Services;
using System;
using System.Threading.Tasks;

#nullable enable
namespace QRCerts.Api.Controllers
{
    [ApiController]
    [Route("api/public")]
    public class PublicController : ControllerBase
    {
        private readonly IAlumnosService _alumnosService;
        private readonly ICursosService _cursosService;
        private readonly ICertificadosService _certificadosService;

        public PublicController(
          IAlumnosService alumnosService,
          ICursosService cursosService,
          ICertificadosService certificadosService)
        {
            this._alumnosService = alumnosService;
            this._cursosService = cursosService;
            this._certificadosService = certificadosService;
        }

        [HttpGet("validar-certificado")]
        public async Task<IActionResult> ValidarCertificado(Guid alumnoId, Guid cursoId)
        {
            PublicController publicController1 = this;
            try
            {
                Alumno alumno = await publicController1._alumnosService.GetByIdAsync(alumnoId);
                if (alumno == null)
                    return (IActionResult)publicController1.Ok((object)new ValidacionCertificadoResponse()
                    {
                        EsValido = false,
                        Mensaje = "El alumno no existe en el sistema"
                    });
                Curso byIdAsync = await publicController1._cursosService.GetByIdAsync(cursoId);
                if (byIdAsync == null)
                    return (IActionResult)publicController1.Ok((object)new ValidacionCertificadoResponse()
                    {
                        EsValido = false,
                        Mensaje = "El curso no existe en el sistema"
                    });
                Certificado byGuid = publicController1._certificadosService.getByGuid(alumnoId, cursoId);
                if (byGuid == null)
                    return (IActionResult)publicController1.Ok((object)new ValidacionCertificadoResponse()
                    {
                        EsValido = false,
                        Mensaje = "No existe un certificado emitido para este alumno en este curso"
                    });
                PublicController publicController2 = publicController1;
                ValidacionCertificadoResponse certificadoResponse = new ValidacionCertificadoResponse();
                certificadoResponse.EsValido = true;
                AlumnoValidacionDto alumnoValidacionDto = new AlumnoValidacionDto();
                Guid id = alumno.Id;
                alumnoValidacionDto.Id = id.ToString();
                alumnoValidacionDto.NombreApellido = alumno.NombreApellido;
                alumnoValidacionDto.RUT = alumno.RUT;
                alumnoValidacionDto.CreatedAt = alumno.CreatedAt;
                certificadoResponse.Alumno = alumnoValidacionDto;
                CursoValidacionDto cursoValidacionDto = new CursoValidacionDto();
                id = byIdAsync.Id;
                cursoValidacionDto.Id = id.ToString();
                cursoValidacionDto.NombreReferencia = byIdAsync.nombre_visualizar_certificado;
                cursoValidacionDto.FondoPath = byIdAsync.FondoPath;
                cursoValidacionDto.CreatedAt = byIdAsync.CreatedAt;
                cursoValidacionDto.nombre_visualizar_certificado =
                    byIdAsync.nombre_visualizar_certificado;
                OtecSummaryDto otecSummaryDto;
                if (byIdAsync.Otec == null)
                {
                    otecSummaryDto = (OtecSummaryDto)null;
                }
                else
                {
                    otecSummaryDto = new OtecSummaryDto();
                    id = byIdAsync.Otec.Id;
                    otecSummaryDto.Id = id.ToString();
                    otecSummaryDto.Nombre = byIdAsync.Otec.Nombre;
                    otecSummaryDto.Slug = byIdAsync.Otec.Slug;
                }
                cursoValidacionDto.Otec = otecSummaryDto;
                certificadoResponse.Curso = cursoValidacionDto;
                certificadoResponse.PdfFilename = byGuid.PdfFilename;
                certificadoResponse.vencimiento = byIdAsync.vencimiento.ToShortDateString();
                return (IActionResult)publicController2.Ok((object)certificadoResponse);
            }
            catch (Exception ex)
            {
                return (IActionResult)publicController1.Ok((object)new ValidacionCertificadoResponse()
                {
                    EsValido = false,
                    Mensaje = ex.Message
                });
            }
        }
    }
}
