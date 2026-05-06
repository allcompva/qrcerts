using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using QRCerts.Api.Controllers.Controllers;
using QRCerts.Api.Models;
using QRCerts.Api.Services;
using QRCoder;
using QRCerts.Api.DTOs;
using DocumentFormat.OpenXml.Packaging;

namespace QRCerts.Api.Controllers
{
    [ApiController]
    [Route("api/app/certificados")]
    public class CertificadosController : Controller
    {
        private ICertificadosService _CertificadosService;

        public CertificadosController(ICertificadosService CertificadosService)
        {
            this._CertificadosService = CertificadosService;
        }

        private string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "certificado";
            string str = Regex.Replace(Regex.Replace(Regex.Replace(text.ToLowerInvariant(), "[^\\w\\s-]", " "), "\\s+", "-"), "-+", "-").Trim('-');
            return !string.IsNullOrWhiteSpace(str) ? str : "certificado";
        }

        private string GeneratePdfFileName(string cursoNombre, string alumnoRut)
        {
            string text = !string.IsNullOrWhiteSpace(cursoNombre) ? cursoNombre : "certificado";
            string str = !string.IsNullOrWhiteSpace(alumnoRut) ? alumnoRut : "sin-rut";
            return this.GenerateSlug(text) + "_" + str.Replace(".", "").Replace("-", "").Replace(" ", "") + ".pdf";
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateCertificadoRequest request, CancellationToken ct)
        {
            CertificadosController certificadosController = this;
            try
            {
                Certificado async = await certificadosController._CertificadosService.CreateAsync(request, ct);
                return (IActionResult)certificadosController.CreatedAtAction("getByGuid", (object)new
                {
                    id = async.Id
                }, (object)async);
            }
            catch (Exception ex)
            {
                return (IActionResult)certificadosController.BadRequest((object)new
                {
                    message = "Error creating curso",
                    error = ex.Message
                });
            }
        }

        [HttpDelete("{idAlumno}/{idCurso}")]
        public async Task<IActionResult> Delete(Guid idAlumno, Guid idCurso, CancellationToken ct)
        {
            CertificadosController certificadosController = this;
            try
            {
                return await certificadosController._CertificadosService.DeleteAsync(idAlumno, idCurso, ct) ? (IActionResult)certificadosController.Ok((object)new
                {
                    message = "Curso deleted successfully"
                }) : (IActionResult)certificadosController.NotFound((object)new
                {
                    message = "Curso not found"
                });
            }
            catch (Exception ex)
            {
                return (IActionResult)certificadosController.BadRequest((object)new
                {
                    message = "Error deleting curso",
                    error = ex.Message
                });
            }
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate(
          [FromBody] GenerateCertificatesRequest request,
          CancellationToken ct)
        {
            CertificadosController certificadosController1 = this;
            try
            {
                foreach (string alumnoId in request.AlumnoIds)
                {
                    Guid result;
                    if (Guid.TryParse(alumnoId, out result) && Guid.TryParse(request.CursoId, out result))
                    {
                        Certificado async = await certificadosController1._CertificadosService.CreateAsync(new CreateCertificadoRequest()
                        {
                            AlumnoId = alumnoId,
                            CursoId = request.CursoId
                        }, ct);
                    }
                }
                CertificadosController certificadosController = certificadosController1;
                DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 1);
                interpolatedStringHandler.AppendLiteral("Se generaron ");
                interpolatedStringHandler.AppendFormatted<int>(request.AlumnoIds.Count);
                interpolatedStringHandler.AppendLiteral(" certificados exitosamente");
                var data = new
                {
                    message = interpolatedStringHandler.ToStringAndClear()
                };
                return (IActionResult)certificadosController.Ok((object)data);
            }
            catch (Exception ex)
            {
                return (IActionResult)certificadosController1.BadRequest((object)new
                {
                    message = "Error al generar certificados",
                    error = ex.Message
                });
            }
        }

        [HttpGet("download/{alumnoId}/{cursoId}")]
        public async Task<IActionResult> Download(
          Guid alumnoId,
          Guid cursoId,
          [FromServices] IAlumnosService alumnosService,
          [FromServices] ICursosService cursosService,
          [FromServices] IWebHostEnvironment webHostEnvironment,
          [FromServices] IGoogleDriveService driveService)
        {
            CertificadosController certificadosController = this;
            try
            {
                Certificado byGuid = certificadosController._CertificadosService.getByGuid(alumnoId, cursoId);
                string fileDownloadName = !string.IsNullOrWhiteSpace(byGuid?.PdfFilename) ? byGuid.PdfFilename : "certificado.pdf";

                // Si tiene fileId en url_landing, descargar de Google Drive
                if (!string.IsNullOrWhiteSpace(byGuid?.url_landing) && byGuid.url_landing.Length > 10 && !byGuid.url_landing.Contains("/"))
                {
                    try
                    {
                        var driveStream = await driveService.DownloadFileAsync(byGuid.url_landing);
                        return File(driveStream, "application/pdf", fileDownloadName);
                    }
                    catch
                    {
                        // Si falla Drive, fallback a generación local
                    }
                }

                // Fallback: generar al vuelo (certificados sin Drive)
                byte[] certificatePdf = await certificadosController.GenerateCertificatePdf(alumnoId, cursoId, alumnosService, cursosService, webHostEnvironment);

                return (IActionResult)certificadosController.File(certificatePdf, "application/pdf", fileDownloadName);
            }
            catch (Exception ex)
            {
                return (IActionResult)certificadosController.BadRequest((object)new
                {
                    message = "Error al generar certificado",
                    error = ex.Message
                });
            }
        }

        [HttpGet("data/{alumnoId}/{cursoId}")]
        public async Task<IActionResult> GetCertificateData(
          Guid alumnoId,
          Guid cursoId,
          [FromServices] IAlumnosService alumnosService,
          [FromServices] ICursosService cursosService)
        {
            CertificadosController certificadosController = this;
            try
            {
                Certificado byGuid = certificadosController._CertificadosService.getByGuid(alumnoId, cursoId);
                if (byGuid == null)
                    return (IActionResult)certificadosController.NotFound((object)new
                    {
                        message = "Certificado no encontrado"
                    });

                // Leer snapshot inmutable desde contenido_certificado
                var contenido = DAL.ContenidoCertificadoDAL.GetByCertificadoId(byGuid.Id);

                if (contenido != null)
                {
                    // Usar datos guardados al momento de emisión (inmutables)
                    var data1 = new
                    {
                        id = alumnoId.ToString(),
                        nombreApellido = contenido.NombreAlumno,
                        rut = contenido.RUT,
                        observaciones = (string?)null
                    };
                    var data2 = new
                    {
                        id = cursoId.ToString(),
                        nombreReferencia = contenido.NombreReferenciaCurso,
                        fondoPath = contenido.ImagenFondo,
                        footer_1 = contenido.Footer1,
                        footer_2 = contenido.Footer2,
                        nombre_visualizar_certificado = contenido.NombreCurso,
                        certificate_type = contenido.CertificateType,
                        contenidoHtml = contenido.ContenidoHtml,
                        footerHtml = contenido.FooterHtml
                    };
                    return (IActionResult)certificadosController.Ok((object)new
                    {
                        alumno = data1,
                        curso = data2,
                        fileName = !string.IsNullOrWhiteSpace(byGuid.PdfFilename) ? byGuid.PdfFilename : "certificado.pdf",
                        plantillaId = (await cursosService.GetByIdAsync(cursoId))?.PlantillaId.ToString() ?? "",
                    });
                }

                // Fallback: si no hay snapshot (certificados antiguos), leer datos actuales
                Alumno alumno = await alumnosService.GetByIdCertAsync(alumnoId, cursoId);
                Curso byIdAsync = await cursosService.GetByIdAsync(cursoId);
                if (alumno == null || byIdAsync == null)
                    return (IActionResult)certificadosController.NotFound((object)new
                    {
                        message = "Alumno o curso no encontrado"
                    });
                string fileName = !string.IsNullOrWhiteSpace(byGuid?.PdfFilename) ? byGuid.PdfFilename : certificadosController.GeneratePdfFileName(byIdAsync.NombreReferencia ?? "certificado", alumno.RUT);
                return (IActionResult)certificadosController.Ok((object)new
                {
                    alumno = new
                    {
                        id = alumno.Id.ToString(),
                        nombreApellido = alumno.NombreApellido,
                        rut = alumno.RUT,
                        observaciones = alumno.observaciones
                    },
                    curso = new
                    {
                        id = byIdAsync.Id.ToString(),
                        nombreReferencia = byIdAsync.NombreReferencia,
                        fondoPath = byIdAsync.FondoPath,
                        footer_1 = byIdAsync.footer_1,
                        footer_2 = byIdAsync.footer_2,
                        nombre_visualizar_certificado = byIdAsync.nombre_visualizar_certificado,
                        certificate_type = byIdAsync.certificate_type,
                        contenidoHtml = byIdAsync.contenidoHtml,
                        footerHtml = byIdAsync.footerHtml
                    },
                    fileName = fileName,
                    plantillaId = byIdAsync.PlantillaId.ToString(),
                });
            }
            catch (Exception ex)
            {
                return (IActionResult)certificadosController.BadRequest((object)new
                {
                    message = "Error al obtener datos del certificado",
                    error = ex.Message
                });
            }
        }

        [HttpPost("download-zip")]
        public async Task<IActionResult> DownloadZip(
          [FromBody] GenerateCertificatesRequest request,
          [FromServices] IAlumnosService alumnosService,
          [FromServices] ICursosService cursosService,
          [FromServices] IWebHostEnvironment webHostEnvironment)
        {
            CertificadosController certificadosController = this;
            try
            {
                byte[] zipBytes = await certificadosController.GenerateCertificatesZip(request.AlumnoIds, Guid.Parse(request.CursoId), alumnosService, cursosService, webHostEnvironment);
                return (IActionResult)certificadosController.File(zipBytes, "application/zip", "certificados_" + (await cursosService.GetByIdAsync(Guid.Parse(request.CursoId)))?.NombreReferencia?.Replace(" ", "_") + ".zip");
            }
            catch (Exception ex)
            {
                return (IActionResult)certificadosController.BadRequest((object)new
                {
                    message = "Error al generar ZIP de certificados",
                    error = ex.Message
                });
            }
        }

        private async Task<byte[]> GenerateCertificatePdfFromData(
          Alumno alumno,
          Curso curso,
          IWebHostEnvironment webHostEnvironment)
        {
            return await GenerateCertificatePdfInternal(alumno, curso, webHostEnvironment);
        }

        private async Task<byte[]> GenerateCertificatePdf(
          Guid alumnoId,
          Guid cursoId,
          IAlumnosService alumnosService,
          ICursosService cursosService,
          IWebHostEnvironment webHostEnvironment)
        {
            CertificadosController certificadosController = this;
            try
            {
                Alumno alumno = await alumnosService.GetByIdAsync(alumnoId);
                Curso byIdAsync = await cursosService.GetByIdAsync(cursoId);
                if (alumno == null || byIdAsync == null)
                    throw new Exception("Alumno o curso no encontrado");
                return await GenerateCertificatePdfInternal(alumno, byIdAsync, webHostEnvironment);
            }
            catch (Exception ex)
            {
                using (MemoryStream os = new MemoryStream())
                {
                    Document document1 = new Document(PageSize.A4);
                    PdfWriter.GetInstance(document1, (Stream)os);
                    document1.Open();
                    document1.Add((IElement)new Paragraph("Error generando certificado: " + ex.Message));
                    document1.Close();
                    return os.ToArray();
                }
            }
        }

        private async Task<byte[]> GenerateCertificatePdfInternal(
          Alumno alumno,
          Curso byIdAsync,
          IWebHostEnvironment webHostEnvironment)
        {
            CertificadosController certificadosController = this;
            try
            {
                iTextSharp.text.Rectangle pageSize = PageSize.A4.Rotate();
                Document document = new Document(pageSize, 20f, 20f, 20f, 20f);
                using (MemoryStream os = new MemoryStream())
                {
                    try
                    {
                        PdfWriter instance1 = PdfWriter.GetInstance(document, (Stream)os);
                        document.Open();
                        PdfContentByte directContent = instance1.DirectContent;
                        if (!string.IsNullOrEmpty(byIdAsync.FondoPath))
                        {
                            string str = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "images", byIdAsync.FondoPath);
                            if (System.IO.File.Exists(str))
                            {
                                try
                                {
                                    Image instance2 = Image.GetInstance(str);
                                    instance2.ScaleToFit(pageSize.Width, pageSize.Height);
                                    instance2.SetAbsolutePosition(0.0f, 0.0f);
                                    directContent.AddImage(instance2);
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                        BaseFont font = BaseFont.CreateFont("Helvetica-Bold", "Cp1252", false);
                        directContent.SetColorFill(BaseColor.BLACK);
                        if (!string.IsNullOrWhiteSpace(alumno.NombreApellido))
                        {
                            string upper = alumno.NombreApellido.ToUpper();
                            float num = 28f;
                            float widthPoint = font.GetWidthPoint(upper, num);
                            float x = (float)(((double)pageSize.Width - (double)widthPoint) / 2.0);
                            float y = pageSize.Height - 300f;
                            directContent.BeginText();
                            directContent.SetFontAndSize(font, num);
                            directContent.SetTextMatrix(x, y);
                            directContent.ShowText(upper);
                            directContent.EndText();
                        }
                        try
                        {
                            Encoding utF8 = Encoding.UTF8;
                            DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
                            interpolatedStringHandler.AppendFormatted<Guid>(alumno.Id);
                            interpolatedStringHandler.AppendLiteral(",");
                            interpolatedStringHandler.AppendFormatted<Guid>(byIdAsync.Id);
                            string stringAndClear = interpolatedStringHandler.ToStringAndClear();
                            string base64String = Convert.ToBase64String(utF8.GetBytes(stringAndClear));
                            interpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 3);
                            interpolatedStringHandler.AppendFormatted(certificadosController.Request.Scheme);
                            interpolatedStringHandler.AppendLiteral("://");
                            interpolatedStringHandler.AppendFormatted<HostString>(certificadosController.Request.Host);
                            interpolatedStringHandler.AppendLiteral("/validar?data=");
                            interpolatedStringHandler.AppendFormatted(base64String);
                            Image instance3 = Image.GetInstance(new PngByteQRCode(new QRCodeGenerator().CreateQrCode(interpolatedStringHandler.ToStringAndClear(), QRCodeGenerator.ECCLevel.Q)).GetGraphic(20));
                            instance3.ScaleToFit(100f, 100f);
                            instance3.SetAbsolutePosition(pageSize.Width - 120f, 20f);
                            directContent.AddImage(instance3);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    finally
                    {
                        if (document.IsOpen())
                            document.Close();
                    }
                    return os.ToArray();
                }
            }
            catch (Exception ex)
            {
                using (MemoryStream os = new MemoryStream())
                {
                    Document document1 = new Document(PageSize.A4);
                    PdfWriter.GetInstance(document1, (Stream)os);
                    document1.Open();
                    document1.Add((IElement)new Paragraph("Error generando certificado: " + ex.Message));
                    Document document2 = document1;
                    DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
                    interpolatedStringHandler.AppendLiteral("Alumno ID: ");
                    interpolatedStringHandler.AppendFormatted<Guid>(alumno.Id);
                    Paragraph paragraph1 = new Paragraph(interpolatedStringHandler.ToStringAndClear());
                    document2.Add((IElement)paragraph1);
                    Document document3 = document1;
                    interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
                    interpolatedStringHandler.AppendLiteral("Curso ID: ");
                    interpolatedStringHandler.AppendFormatted<Guid>(byIdAsync.Id);
                    Paragraph paragraph2 = new Paragraph(interpolatedStringHandler.ToStringAndClear());
                    document3.Add((IElement)paragraph2);
                    document1.Close();
                    return os.ToArray();
                }
            }
        }

        private async Task<byte[]> GenerateCertificatesZip(
          List<string> alumnoIds,
          Guid cursoId,
          IAlumnosService alumnosService,
          ICursosService cursosService,
          IWebHostEnvironment webHostEnvironment)
        {
            ZipArchive archive;
            List<object> manifestEntries;
            Stream manifestStream;
            StreamWriter writer;
            byte[] array;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                archive = new ZipArchive((Stream)memoryStream, ZipArchiveMode.Create, true);
                try
                {
                    manifestEntries = new List<object>();
                    foreach (string alumnoId1 in alumnoIds)
                    {
                        Guid alumnoId;
                        if (Guid.TryParse(alumnoId1, out alumnoId))
                        {
                            Alumno alumno = await alumnosService.GetByIdAsync(alumnoId);
                            if (alumno != null)
                            {
                                byte[] pdfBytes = await this.GenerateCertificatePdf(alumnoId, cursoId, alumnosService, cursosService, webHostEnvironment);
                                string fileName = this.GeneratePdfFileName((await cursosService.GetByIdAsync(cursoId))?.NombreReferencia ?? "certificado", alumno.RUT);
                                Stream entryStream = archive.CreateEntry(fileName).Open();
                                try
                                {
                                    await entryStream.WriteAsync(pdfBytes, 0, pdfBytes.Length);
                                    manifestEntries.Add((object)new
                                    {
                                        fileName = fileName,
                                        alumno = alumno.NombreApellido,
                                        rut = alumno.RUT,
                                        generatedAt = DateTime.UtcNow
                                    });
                                }
                                finally
                                {
                                    entryStream?.Dispose();
                                }
                                pdfBytes = (byte[])null;
                                fileName = (string)null;
                                entryStream = (Stream)null;
                                pdfBytes = (byte[])null;
                                fileName = (string)null;
                                entryStream = (Stream)null;
                            }
                            alumno = (Alumno)null;
                            alumno = (Alumno)null;
                        }
                    }
                    string str = JsonSerializer.Serialize(new
                    {
                        generatedAt = DateTime.UtcNow,
                        totalFiles = manifestEntries.Count,
                        files = manifestEntries
                    }, new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    });
                    manifestStream = archive.CreateEntry("manifest.json").Open();
                    try
                    {
                        writer = new StreamWriter(manifestStream);
                        try
                        {
                            await writer.WriteAsync(str);
                            array = memoryStream.ToArray();
                        }
                        finally
                        {
                            writer?.Dispose();
                        }
                    }
                    finally
                    {
                        manifestStream?.Dispose();
                    }
                }
                finally
                {
                    archive?.Dispose();
                }
            }
            archive = (ZipArchive)null;
            manifestEntries = (List<object>)null;
            manifestStream = (Stream)null;
            writer = (StreamWriter)null;
            byte[] certificatesZip = array;
            archive = (ZipArchive)null;
            manifestEntries = (List<object>)null;
            manifestStream = (Stream)null;
            writer = (StreamWriter)null;
            return certificatesZip;
        }
    }
}