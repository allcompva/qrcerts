// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Controllers.AlumnosController
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCerts.Api.DAL;
using QRCerts.Api.DTOs;
using QRCerts.Api.Models;
using QRCerts.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace QRCerts.Api.Controllers
{
    [ApiController]
    [Route("api/app/alumnos")]
    [Authorize]
    public class AlumnosController : ControllerBase
    {
        private readonly IAlumnosService _alumnosService;
        private readonly IOtecService _otecService;
        private readonly IRegistroAlumnosService _registroAlumnosService;

        public AlumnosController(
          IAlumnosService alumnosService,
          IOtecService otecService,
          IRegistroAlumnosService registroAlumnosService)
        {
            this._alumnosService = alumnosService;
            this._otecService = otecService;
            this._registroAlumnosService = registroAlumnosService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            AlumnosController alumnosController = this;
            string input = alumnosController.User.FindFirst("otec_id")?.Value;
            Guid result;
            if (string.IsNullOrEmpty(input) || !Guid.TryParse(input, out result))
                return (IActionResult)alumnosController.Unauthorized((object)new
                {
                    message = "Invalid OTEC context"
                });
            List<AlumnoDto> list = (await alumnosController._alumnosService.GetByOtecIdAsync(result)).Select<Alumno, AlumnoDto>((Func<Alumno, AlumnoDto>)(a =>
            {
                AlumnoDto all = new AlumnoDto();
                all.Id = a.Id.ToString();
                all.NombreApellido = a.NombreApellido;
                all.RUT = a.RUT;
                all.CreatedAt = a.CreatedAt;
                OtecSummaryDto otecSummaryDto;
                if (a.Otec == null)
                {
                    otecSummaryDto = (OtecSummaryDto)null;
                }
                else
                {
                    otecSummaryDto = new OtecSummaryDto();
                    otecSummaryDto.Id = a.Otec.Id.ToString();
                    otecSummaryDto.Nombre = a.Otec.Nombre;
                    otecSummaryDto.Slug = a.Otec.Slug;
                }
                all.Otec = otecSummaryDto;
                return all;
            })).ToList<AlumnoDto>();
            return (IActionResult)alumnosController.Ok((object)list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            AlumnosController alumnosController = this;
            string input = alumnosController.User.FindFirst("otec_id")?.Value;
            Guid otecId;
            if (string.IsNullOrEmpty(input) || !Guid.TryParse(input, out otecId))
                return (IActionResult)alumnosController.Unauthorized((object)new
                {
                    message = "Invalid OTEC context"
                });
            Alumno byIdAsync = await alumnosController._alumnosService.GetByIdAsync(id);
            if (byIdAsync == null || byIdAsync.OtecId != otecId)
                return (IActionResult)alumnosController.NotFound((object)new
                {
                    message = "Alumno not found"
                });
            AlumnoDetailDto alumnoDetailDto1 = new AlumnoDetailDto();
            Guid id1 = byIdAsync.Id;
            alumnoDetailDto1.Id = id1.ToString();
            alumnoDetailDto1.NombreApellido = byIdAsync.NombreApellido;
            alumnoDetailDto1.RUT = byIdAsync.RUT;
            alumnoDetailDto1.CreatedAt = byIdAsync.CreatedAt;
            OtecSummaryDto otecSummaryDto;
            if (byIdAsync.Otec == null)
            {
                otecSummaryDto = (OtecSummaryDto)null;
            }
            else
            {
                otecSummaryDto = new OtecSummaryDto();
                id1 = byIdAsync.Otec.Id;
                otecSummaryDto.Id = id1.ToString();
                otecSummaryDto.Nombre = byIdAsync.Otec.Nombre;
                otecSummaryDto.Slug = byIdAsync.Otec.Slug;
            }
            alumnoDetailDto1.Otec = otecSummaryDto;
            AlumnoDetailDto alumnoDetailDto2 = alumnoDetailDto1;
            return (IActionResult)alumnosController.Ok((object)alumnoDetailDto2);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrUpdate([FromBody] QRCerts.Api.DTOs.CreateAlumnoRequest request)
        {
            AlumnosController alumnosController1 = this;
            string input = alumnosController1.User.FindFirst("otec_id")?.Value;
            Guid otecId;
            if (string.IsNullOrEmpty(input) || !Guid.TryParse(input, out otecId))
                return (IActionResult)alumnosController1.Unauthorized((object)new
                {
                    message = "Invalid OTEC context"
                });
            if (!string.IsNullOrEmpty(request.Id))
            {
                UpdateAlumnoRequest request1 = new UpdateAlumnoRequest()
                {
                    NombreApellido = request.NombreApellido,
                    RUT = request.rut
                };
                Alumno alumno = await alumnosController1._alumnosService.UpdateAsync(Guid.Parse(request.Id), request1);
                if (alumno == null || alumno.OtecId != otecId)
                    return (IActionResult)alumnosController1.NotFound((object)new
                    {
                        message = "Alumno not found"
                    });
                AlumnosController alumnosController2 = alumnosController1;
                AlumnoDetailDto alumnoDetailDto = new AlumnoDetailDto();
                alumnoDetailDto.Id = alumno.Id.ToString();
                alumnoDetailDto.NombreApellido = alumno.NombreApellido;
                alumnoDetailDto.RUT = alumno.RUT;
                alumnoDetailDto.CreatedAt = alumno.CreatedAt;
                OtecSummaryDto otecSummaryDto;
                if (alumno.Otec == null)
                {
                    otecSummaryDto = (OtecSummaryDto)null;
                }
                else
                {
                    otecSummaryDto = new OtecSummaryDto();
                    otecSummaryDto.Id = alumno.Otec.Id.ToString();
                    otecSummaryDto.Nombre = alumno.Otec.Nombre;
                    otecSummaryDto.Slug = alumno.Otec.Slug;
                }
                alumnoDetailDto.Otec = otecSummaryDto;
                return (IActionResult)alumnosController2.Ok((object)alumnoDetailDto);
            }
            QRCerts.Api.DTOs.CreateAlumnoRequest request2 = new QRCerts.Api.DTOs.CreateAlumnoRequest()
            {
                OtecId = otecId.ToString(),
                NombreApellido = request.NombreApellido,
                rut = request.rut
            };
            Alumno async = await alumnosController1._alumnosService.CreateAsync(request2);
            AlumnosController alumnosController3 = alumnosController1;
            var routeValues = new { id = async.Id };
            AlumnoDetailDto alumnoDetailDto1 = new AlumnoDetailDto();
            alumnoDetailDto1.Id = async.Id.ToString();
            alumnoDetailDto1.NombreApellido = async.NombreApellido;
            alumnoDetailDto1.RUT = async.RUT;
            alumnoDetailDto1.CreatedAt = async.CreatedAt;
            OtecSummaryDto otecSummaryDto1;
            if (async.Otec == null)
            {
                otecSummaryDto1 = (OtecSummaryDto)null;
            }
            else
            {
                otecSummaryDto1 = new OtecSummaryDto();
                otecSummaryDto1.Id = async.Otec.Id.ToString();
                otecSummaryDto1.Nombre = async.Otec.Nombre;
                otecSummaryDto1.Slug = async.Otec.Slug;
            }
            alumnoDetailDto1.Otec = otecSummaryDto1;
            return (IActionResult)alumnosController3.CreatedAtAction("GetById", (object)routeValues, (object)alumnoDetailDto1);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            AlumnosController alumnosController = this;
            string input = alumnosController.User.FindFirst("otec_id")?.Value;
            Guid otecId;
            if (string.IsNullOrEmpty(input) || !Guid.TryParse(input, out otecId))
                return (IActionResult)alumnosController.Unauthorized((object)new
                {
                    message = "Invalid OTEC context"
                });
            Alumno byIdAsync = await alumnosController._alumnosService.GetByIdAsync(id);
            return byIdAsync == null || byIdAsync.OtecId != otecId ? (IActionResult)alumnosController.NotFound((object)new
            {
                message = "Alumno not found"
            }) : (await alumnosController._alumnosService.DeleteAsync(id) ? (IActionResult)alumnosController.Ok((object)new
            {
                message = "Alumno deleted successfully"
            }) : (IActionResult)alumnosController.NotFound((object)new
            {
                message = "Alumno not found"
            }));
        }

        [HttpGet("GetByCursoId/{cursoId}")]
        public async Task<IActionResult> GetByCursoId(Guid cursoId, CancellationToken ct)
        {
            AlumnosController alumnosController = this;
            List<AlumnoWithRegistroDto> list = (await alumnosController._alumnosService.GetByCursoIdAsync(cursoId, ct)).Select<Alumno, AlumnoWithRegistroDto>((Func<Alumno, AlumnoWithRegistroDto>)(a =>
            {
                AlumnoWithRegistroDto byCursoId = new AlumnoWithRegistroDto();
                byCursoId.Id = a.Id.ToString();
                byCursoId.NombreApellido = a.NombreApellido;
                byCursoId.RUT = a.RUT;
                byCursoId.CreatedAt = a.CreatedAt;
                byCursoId.certificado = a.certificado;
                byCursoId.Observaciones = a.observaciones;
                OtecSummaryDto otecSummaryDto;
                if (a.Otec == null)
                {
                    otecSummaryDto = (OtecSummaryDto)null;
                }
                else
                {
                    otecSummaryDto = new OtecSummaryDto();
                    otecSummaryDto.Id = a.Otec.Id.ToString();
                    otecSummaryDto.Nombre = a.Otec.Nombre;
                    otecSummaryDto.Slug = a.Otec.Slug;
                }
                byCursoId.Otec = otecSummaryDto;
                return byCursoId;
            })).ToList<AlumnoWithRegistroDto>();
            return (IActionResult)alumnosController.Ok((object)list);
        }

        [HttpPost("create-with-registro")]
        public async Task<IActionResult> CreateWithRegistro([FromBody] CreateAlumnoWithRegistroRequest request)
        {
            AlumnosController alumnosController1 = this;
            string input = alumnosController1.User.FindFirst("otec_id")?.Value;
            Guid result;
            if (string.IsNullOrEmpty(input) || !Guid.TryParse(input, out result))
                return (IActionResult)alumnosController1.Unauthorized((object)new
                {
                    message = "Invalid OTEC context"
                });
            Guid cursoGuid = Guid.Parse(request.CursoId);

            // Validar duplicado en este curso
            if (AlumnosDAL.VerificaDuplicado(request.RUT, cursoGuid.ToString()))
                return (IActionResult)alumnosController1.BadRequest((object)new
                {
                    message = "El alumno con este RUT ya está registrado en este curso."
                });

            // Reusar alumno existente por RUT+OtecId (sin duplicar)
            var alumnoExistente = AlumnosDAL.GetByOtecIdAndRut(result, request.RUT);
            Alumno async;
            if (alumnoExistente != null)
            {
                if (!string.IsNullOrWhiteSpace(request.NombreApellido) && alumnoExistente.NombreApellido != request.NombreApellido)
                {
                    alumnoExistente.NombreApellido = request.NombreApellido;
                    AlumnosDAL.Update(alumnoExistente);
                }
                async = alumnoExistente;
            }
            else
            {
                QRCerts.Api.DTOs.CreateAlumnoRequest request1 = new QRCerts.Api.DTOs.CreateAlumnoRequest()
                {
                    OtecId = result.ToString(),
                    NombreApellido = request.NombreApellido,
                    rut = request.RUT,
                    parametros = request.parametros
                };
                async = await alumnosController1._alumnosService.CreateAsync(request1);
            }

            REGISTRO_ALUMNOS registroAlumnos = new REGISTRO_ALUMNOS()
            {
                id_alumno = async.Id,
                id_curso = cursoGuid,
                observaciones = request.observaciones,
            };
            alumnosController1._registroAlumnosService.Insert(registroAlumnos);
            AlumnosController alumnosController2 = alumnosController1;
            var routeValues = new { id = async.Id };
            AlumnoWithRegistroDto alumnoWithRegistroDto = new AlumnoWithRegistroDto();
            alumnoWithRegistroDto.Id = async.Id.ToString();
            alumnoWithRegistroDto.NombreApellido = async.NombreApellido;
            alumnoWithRegistroDto.RUT = async.RUT;
            alumnoWithRegistroDto.CreatedAt = async.CreatedAt;
            OtecSummaryDto otecSummaryDto;
            if (async.Otec == null)
            {
                otecSummaryDto = (OtecSummaryDto)null;
            }
            else
            {
                otecSummaryDto = new OtecSummaryDto();
                otecSummaryDto.Id = async.Otec.Id.ToString();
                otecSummaryDto.Nombre = async.Otec.Nombre;
                otecSummaryDto.Slug = async.Otec.Slug;
            }
            alumnoWithRegistroDto.Otec = otecSummaryDto;
            return (IActionResult)alumnosController2.CreatedAtAction("GetById", (object)routeValues, (object)alumnoWithRegistroDto);
        }

        [HttpPut("{alumnoId}/registro/{cursoId}")]
        public async Task<IActionResult> UpdateRegistro(
          Guid alumnoId,
          Guid cursoId,
          [FromBody] UpdateAlumnoRegistroRequest request)
        {
            AlumnosController alumnosController1 = this;
            string input = alumnosController1.User.FindFirst("otec_id")?.Value;
            if (string.IsNullOrEmpty(input) || !Guid.TryParse(input, out Guid _))
                return (IActionResult)alumnosController1.Unauthorized((object)new
                {
                    message = "Invalid OTEC context"
                });
            UpdateAlumnoRequest request1 = new UpdateAlumnoRequest()
            {
                NombreApellido = request.NombreApellido,
                RUT = request.RUT
            };
            Alumno alumno = await alumnosController1._alumnosService.UpdateAsync(alumnoId, request1);
            // Actualizar observaciones en REGISTRO_ALUMNOS sin borrar/recrear
            if (request.observaciones != null)
            {
                DAL.REGISTRO_ALUMNOS.Update(alumnoId, cursoId, request.observaciones);
            }
            AlumnosController alumnosController2 = alumnosController1;
            AlumnoDetailDto alumnoDetailDto = new AlumnoDetailDto();
            alumnoDetailDto.Id = alumno.Id.ToString();
            alumnoDetailDto.NombreApellido = alumno.NombreApellido;
            alumnoDetailDto.RUT = alumno.RUT;
            alumnoDetailDto.CreatedAt = alumno.CreatedAt;
            OtecSummaryDto otecSummaryDto;
            if (alumno.Otec == null)
            {
                otecSummaryDto = (OtecSummaryDto)null;
            }
            else
            {
                otecSummaryDto = new OtecSummaryDto();
                otecSummaryDto.Id = alumno.Otec.Id.ToString();
                otecSummaryDto.Nombre = alumno.Otec.Nombre;
                otecSummaryDto.Slug = alumno.Otec.Slug;
            }
            alumnoDetailDto.Otec = otecSummaryDto;
            return (IActionResult)alumnosController2.Ok((object)alumnoDetailDto);
        }

        [HttpDelete("{alumnoId}/registro/{cursoId}")]
        public async Task<IActionResult> DeleteRegistro(Guid alumnoId, Guid cursoId)
        {
            AlumnosController alumnosController = this;
            string input = alumnosController.User.FindFirst("otec_id")?.Value;
            Guid otecId;
            if (string.IsNullOrEmpty(input) || !Guid.TryParse(input, out otecId))
                return (IActionResult)alumnosController.Unauthorized((object)new
                {
                    message = "Invalid OTEC context"
                });
            Alumno byIdAsync = await alumnosController._alumnosService.GetByIdAsync(alumnoId);
            if (byIdAsync == null || byIdAsync.OtecId != otecId)
                return (IActionResult)alumnosController.NotFound((object)new
                {
                    message = "Alumno not found"
                });
            alumnosController._registroAlumnosService.delete(alumnoId, cursoId);
            return (IActionResult)alumnosController.Ok((object)new
            {
                message = "Registro de alumno eliminado exitosamente"
            });
        }

        [HttpPost("bulk-import/{cursoId}")]
        public async Task<IActionResult> BulkImport(Guid cursoId, [FromBody] BulkImportRequest request)
        {
            AlumnosController alumnosController1 = this;
            string input = alumnosController1.User.FindFirst("otec_id")?.Value;
            Guid otecId;
            if (string.IsNullOrEmpty(input) || !Guid.TryParse(input, out otecId))
                return (IActionResult)alumnosController1.Unauthorized((object)new
                {
                    message = "Invalid OTEC context"
                });
            try
            {
                List<AlumnoWithRegistroDto> results = new List<AlumnoWithRegistroDto>();
                foreach (BulkImportAlumnoRequest item in request.Alumnos)
                {
                    // Saltar si ya está inscrito en este curso
                    if (AlumnosDAL.VerificaDuplicado(item.RUT, cursoId.ToString()))
                        continue;

                    // Buscar alumno existente por RUT + OtecId para no duplicar
                    var alumnoExistente = AlumnosDAL.GetByOtecIdAndRut(otecId, item.RUT);
                    Alumno async;

                    if (alumnoExistente != null)
                    {
                        // Reusar alumno existente, actualizar nombre si cambió
                        if (!string.IsNullOrWhiteSpace(item.NombreApellido) && alumnoExistente.NombreApellido != item.NombreApellido)
                        {
                            alumnoExistente.NombreApellido = item.NombreApellido;
                            AlumnosDAL.Update(alumnoExistente);
                        }
                        async = alumnoExistente;
                    }
                    else
                    {
                        QRCerts.Api.DTOs.CreateAlumnoRequest request1 = new QRCerts.Api.DTOs.CreateAlumnoRequest()
                        {
                            OtecId = otecId.ToString(),
                            NombreApellido = item.NombreApellido,
                            rut = item.RUT,
                        };
                        async = await alumnosController1._alumnosService.CreateAsync(request1);
                    }

                    REGISTRO_ALUMNOS registroAlumnos = new REGISTRO_ALUMNOS()
                    {
                        id_alumno = async.Id,
                        id_curso = cursoId,
                        observaciones = item.Observaciones,
                        MoodleUserId = item.MoodleUserId,
                        MoodleCourseId = item.MoodleCourseId,
                    };
                    alumnosController1._registroAlumnosService.Insert(registroAlumnos);
                    List<AlumnoWithRegistroDto> alumnoWithRegistroDtoList = results;
                    AlumnoWithRegistroDto alumnoWithRegistroDto = new AlumnoWithRegistroDto();
                    alumnoWithRegistroDto.Id = async.Id.ToString();
                    alumnoWithRegistroDto.NombreApellido = async.NombreApellido;
                    alumnoWithRegistroDto.RUT = async.RUT;
                    alumnoWithRegistroDto.CreatedAt = async.CreatedAt;
                    alumnoWithRegistroDto.Calificacion = item.Calificacion;
                    alumnoWithRegistroDto.Observaciones = item.Observaciones;
                    alumnoWithRegistroDto.certificado_otorgado = item.certificado_otorgado;
                    alumnoWithRegistroDto.motivo_entrega = item.motivo_entrega;
                    OtecSummaryDto otecSummaryDto;
                    if (async.Otec == null)
                    {
                        otecSummaryDto = (OtecSummaryDto)null;
                    }
                    else
                    {
                        otecSummaryDto = new OtecSummaryDto();
                        otecSummaryDto.Id = async.Otec.Id.ToString();
                        otecSummaryDto.Nombre = async.Otec.Nombre;
                        otecSummaryDto.Slug = async.Otec.Slug;
                    }
                    alumnoWithRegistroDto.Otec = otecSummaryDto;
                    alumnoWithRegistroDtoList.Add(alumnoWithRegistroDto);
                }
                AlumnosController alumnosController2 = alumnosController1;
                DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(35, 1);
                interpolatedStringHandler.AppendLiteral("Se importaron ");
                interpolatedStringHandler.AppendFormatted<int>(results.Count);
                interpolatedStringHandler.AppendLiteral(" alumnos exitosamente");
                var data = new
                {
                    message = interpolatedStringHandler.ToStringAndClear(),
                    alumnos = results
                };
                return (IActionResult)alumnosController2.Ok((object)data);
            }
            catch (Exception ex)
            {
                return (IActionResult)alumnosController1.BadRequest((object)new
                {
                    message = "Error en la importación masiva",
                    error = ex.Message
                });
            }
        }
    }
}
