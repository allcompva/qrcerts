using QRCerts.Api.DAL;
using QRCerts.Api.Models;
using QRCerts.Api.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace QRCerts.Api.Services
{
  public class CertificadosService : ICertificadosService
  {
    private readonly IQuotaService _quotaService;
    private readonly IGoogleDriveService _driveService;
    private readonly IPlantillaCertificadosService _plantillaService;
    private readonly IWebHostEnvironment _env;
    private readonly string _qrBaseUrl;

    public CertificadosService(
      IQuotaService quotaService,
      IGoogleDriveService driveService,
      IPlantillaCertificadosService plantillaService,
      IWebHostEnvironment env,
      IConfiguration configuration)
    {
      _quotaService = quotaService;
      _driveService = driveService;
      _plantillaService = plantillaService;
      _env = env;
      _qrBaseUrl = configuration["QrBaseUrl"] ?? "https://certificadosqr.store/app/#/validar?data=";
    }

    public Certificado getByPk(int Id) => CertificadoDAL.getByPk(Id);
    public Certificado getByGuid(Guid AlumnoId, Guid CursoId) => CertificadoDAL.getByGuid(AlumnoId, CursoId);
    public List<Certificado> read() => CertificadoDAL.read();
    public int insert(Certificado obj) => CertificadoDAL.insert(obj);
    public void update(Certificado obj) => CertificadoDAL.update(obj);
    public void delete(Guid idAlumno, Guid idCurso) => CertificadoDAL.delete(idAlumno, idCurso);

    public async Task<Certificado> CreateAsync(
      CreateCertificadoRequest request,
      CancellationToken cancellationToken = default)
    {
      Guid alumnoId = string.IsNullOrEmpty(request.AlumnoId) ? Guid.Empty : Guid.Parse(request.AlumnoId);
      Guid cursoId = string.IsNullOrEmpty(request.CursoId) ? Guid.Empty : Guid.Parse(request.CursoId);

      var alumno = AlumnosDAL.GetById(alumnoId);
      var curso = CursosDAL.GetById(cursoId);

      if (alumno == null || curso == null)
        throw new ArgumentException("Alumno o curso no encontrado");

      // Validar quota
      if (!_quotaService.PuedeEmitir(curso.OtecId))
        throw new InvalidOperationException("No dispone de saldo suficiente para emitir certificados. Contacte al administrador del sitio.");

      // Generar nombre del archivo: nombre-alumno_slug-curso.pdf
      var pdfFilename = GeneratePdfFilename(alumno.NombreApellido, curso.NombreReferencia);

      // Crear registro en BD
      var cert = new Certificado
      {
        Id = Guid.NewGuid(),
        CursoId = cursoId,
        AlumnoId = alumnoId,
        Code = request.Code,
        PdfFilename = pdfFilename,
        IssuedAt = DateTime.Now,
        url_landing = ""
      };
      CertificadoDAL.insert(cert);

      // Generar PDF y subir a Drive usando el mismo método que funciona en generate-pdf-by-ids
      try
      {
        var webRootPath = _env.WebRootPath ?? Directory.GetCurrentDirectory();
        Console.WriteLine($"[Emit] Generando PDF - alumno:{alumnoId} curso:{cursoId} webRoot:{webRootPath}");
        var pdfPath = await UploadController.GeneratePdfToFile(
          cursoId, alumnoId, _plantillaService, webRootPath, _qrBaseUrl, cancellationToken);

        if (!string.IsNullOrEmpty(pdfPath) && File.Exists(pdfPath))
        {
          // Obtener slug del OTEC para la carpeta
          var otecSlug = "default";
          var otec = OtecDAL.GetById(curso.OtecId);
          if (otec != null) otecSlug = otec.Slug;

          Console.WriteLine($"[Drive] Subiendo {pdfFilename} a Certificados/{otecSlug}...");

          using var fileStream = File.OpenRead(pdfPath);
          var driveFileId = await _driveService.UploadFileAsync(
            fileStream, pdfFilename, $"Certificados/{otecSlug}", ct: cancellationToken);

          if (!string.IsNullOrEmpty(driveFileId))
          {
            cert.url_landing = driveFileId;
            CertificadoDAL.update(cert);
            Console.WriteLine($"[Drive] OK FileId: {driveFileId}");
          }

          // Limpiar PDF temporal
          try { File.Delete(pdfPath); } catch { }
        }
        else
        {
          Console.WriteLine($"[Drive] No se generó PDF para {alumnoId}/{cursoId}");
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"[Drive] Error: {ex.Message}");
      }

      // Consumir quota
      _quotaService.ConsumirQuota(curso.OtecId);

      return cert;
    }

    private static string GeneratePdfFilename(string nombreAlumno, string nombreCurso)
    {
      var alumnoSlug = Slugify(nombreAlumno);
      var cursoSlug = Slugify(nombreCurso);
      return $"{alumnoSlug}_{cursoSlug}.pdf";
    }

    private static string Slugify(string text)
    {
      if (string.IsNullOrWhiteSpace(text)) return "sin-datos";
      return text.ToLower()
        .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
        .Replace("ñ", "n").Replace("ü", "u")
        .Replace(" ", "-")
        .Replace(".", "").Replace(",", "").Replace(";", "").Replace(":", "")
        .Replace("(", "").Replace(")", "").Replace("'", "").Replace("\"", "")
        .Trim('-');
    }

    public async Task<bool> DeleteAsync(
      Guid idAlumno,
      Guid idCurso,
      CancellationToken cancellationToken = default)
    {
      var cert = CertificadoDAL.getByGuid(idAlumno, idCurso);
      if (cert != null && !string.IsNullOrWhiteSpace(cert.url_landing) && cert.url_landing.Length > 10 && !cert.url_landing.Contains("/"))
      {
        try
        {
          var curso = CursosDAL.GetById(idCurso);
          var otecSlug = "default";
          if (curso != null)
          {
            var otec = OtecDAL.GetById(curso.OtecId);
            if (otec != null) otecSlug = otec.Slug;
          }
          await _driveService.MoveToHistoricoAsync(cert.url_landing, otecSlug, cancellationToken);
        }
        catch { }
      }

      CertificadoDAL.delete(idAlumno, idCurso);
      return true;
    }
  }
}
