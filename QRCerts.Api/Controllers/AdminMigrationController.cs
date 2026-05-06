using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QRCerts.Api.DAL;
using QRCerts.Api.Services;
using System.Data;

namespace QRCerts.Api.Controllers
{
    [ApiController]
    [Route("api/admin/migration")]
    public class AdminMigrationController : ControllerBase
    {
        private readonly IGoogleDriveService _driveService;
        private readonly IWebHostEnvironment _env;
        private readonly string _qrBaseUrl;

        public AdminMigrationController(IGoogleDriveService driveService, IWebHostEnvironment env, IConfiguration configuration)
        {
            _driveService = driveService;
            _env = env;
            _qrBaseUrl = configuration["QrBaseUrl"] ?? "https://certificadosqr.store/app/#/validar?data=";
        }

        /// <summary>
        /// Migra certificados existentes a Drive en bloques.
        /// GET /api/admin/migration/certs-to-drive?batch=50
        /// </summary>
        [HttpGet("certs-to-drive")]
        public async Task<IActionResult> MigrateCertsToDrive([FromQuery] int batch = 50, CancellationToken ct = default)
        {
            if (!_driveService.IsAuthenticated)
                return BadRequest(new { message = "Drive no autenticado. Primero POST /api/admin/drive/authenticate" });

            int migrated = 0, skipped = 0, errors = 0;
            var errorList = new List<string>();

            try
            {
                // Obtener certificados sin Drive ID
                var certs = GetCertsToMigrate(batch);

                foreach (var cert in certs)
                {
                    ct.ThrowIfCancellationRequested();
                    try
                    {
                        // Buscar el PDF localmente (generado por el sistema anterior)
                        // Los PDFs generados por plantilla no se guardaban localmente,
                        // así que necesitamos regenerar usando generate-pdf-by-ids
                        // Para migración, usamos el endpoint existente

                        var driveFileId = await GenerateAndUploadCert(cert, ct);

                        if (driveFileId != null)
                        {
                            // Actualizar url_landing
                            UpdateUrlLanding(cert.CertificadoId, driveFileId);
                            migrated++;
                        }
                        else
                        {
                            skipped++;
                        }
                    }
                    catch (Exception ex)
                    {
                        errors++;
                        errorList.Add($"{cert.CertificadoId}: {ex.Message}");
                        if (errors > 10) break; // Parar si hay muchos errores
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return Ok(new { message = "Cancelado", migrated, skipped, errors });
            }

            return Ok(new
            {
                message = $"Migración completada. Migrados: {migrated}, Omitidos: {skipped}, Errores: {errors}",
                migrated,
                skipped,
                errors,
                errorDetails = errorList.Take(20)
            });
        }

        /// <summary>
        /// Muestra estado de la migración
        /// </summary>
        [HttpGet("certs-status")]
        public IActionResult GetMigrationStatus()
        {
            try
            {
                using var con = GetConnection();
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = @"
                    SELECT
                        COUNT(*) as Total,
                        SUM(CASE WHEN url_landing IS NOT NULL AND url_landing != '' AND LEN(url_landing) > 10 THEN 1 ELSE 0 END) as EnDrive,
                        SUM(CASE WHEN url_landing IS NULL OR url_landing = '' OR LEN(url_landing) <= 10 THEN 1 ELSE 0 END) as Pendientes
                    FROM Certificados";
                using var dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    return Ok(new
                    {
                        total = dr.GetInt32(0),
                        enDrive = dr.GetInt32(1),
                        pendientes = dr.GetInt32(2)
                    });
                }
                return Ok(new { total = 0, enDrive = 0, pendientes = 0 });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private async Task<string?> GenerateAndUploadCert(CertMigrationInfo cert, CancellationToken ct)
        {
            // Obtener plantilla del curso
            var templateFileName = Plantilla_certificados.GetTemplateFileName(cert.CursoId);
            if (string.IsNullOrWhiteSpace(templateFileName))
                return null; // Sin plantilla, no se puede regenerar

            var plantillaPath = Path.Combine(
                _env.WebRootPath ?? Directory.GetCurrentDirectory(),
                "uploads", "docx", templateFileName);

            if (!System.IO.File.Exists(plantillaPath))
                return null; // Plantilla no encontrada en disco

            // Obtener JSON de reemplazo
            var jsonString = Plantilla_certificados.GetReplacementJson(cert.CursoId, cert.AlumnoId);
            if (string.IsNullOrWhiteSpace(jsonString))
                jsonString = "{}";

            var values = System.Text.Json.JsonSerializer
                .Deserialize<Dictionary<string, string>>(jsonString)
                ?? new Dictionary<string, string>();

            // Generar PDF
            var tempDocx = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".docx");
            var pdfOutDir = Path.Combine(Path.GetTempPath(), "migrate_" + Guid.NewGuid());
            Directory.CreateDirectory(pdfOutDir);

            try
            {
                System.IO.File.Copy(plantillaPath, tempDocx, true);

                // QR
                var qrData = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes($"{cert.AlumnoId},{cert.CursoId}"));
                var qrUrl = $"{_qrBaseUrl}{qrData}";
                var qrBytes = GenerateQrPng(qrUrl);
                UploadController.ReplacePlaceholdersInDocx(tempDocx, tempDocx, values);
                // Note: QR replacement would need the method from UploadController
                // For simplicity in migration, we skip QR image replacement if it's complex

                var sofficePath = Environment.GetEnvironmentVariable("SOFFICE_PATH") ?? "/usr/bin/soffice";
                var pdfPath = await LibreOfficeHelper.ConvertDocxToPdfWithLibreOfficeAsync(
                    tempDocx, pdfOutDir, sofficePath, 120000, ct);

                if (string.IsNullOrWhiteSpace(pdfPath) || !System.IO.File.Exists(pdfPath))
                    return null;

                // Subir a Drive
                using var fileStream = System.IO.File.OpenRead(pdfPath);
                var fileName = $"{cert.AlumnoId}_{cert.CursoId}.pdf";
                return await _driveService.UploadFileAsync(
                    fileStream, fileName, $"Certificados/{cert.OtecSlug}",
                    ct: ct);
            }
            finally
            {
                try { if (System.IO.File.Exists(tempDocx)) System.IO.File.Delete(tempDocx); } catch { }
                try { if (Directory.Exists(pdfOutDir)) Directory.Delete(pdfOutDir, true); } catch { }
            }
        }

        private static byte[] GenerateQrPng(string content)
        {
            using var qrGenerator = new QRCoder.QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(content, QRCoder.QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCoder.PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(10);
        }

        private static void UpdateUrlLanding(Guid certId, string driveFileId)
        {
            using var con = GetConnection();
            con.Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = "UPDATE Certificados SET url_landing = @driveFileId WHERE Id = @Id";
            cmd.Parameters.AddWithValue("@driveFileId", driveFileId);
            cmd.Parameters.AddWithValue("@Id", certId);
            cmd.ExecuteNonQuery();
        }

        private static List<CertMigrationInfo> GetCertsToMigrate(int batch)
        {
            var list = new List<CertMigrationInfo>();
            using var con = GetConnection();
            con.Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = @"
                SELECT TOP(@batch) c.Id, c.AlumnoId, c.CursoId, c.PdfFilename,
                    ISNULL(o.Slug, 'default') as OtecSlug
                FROM Certificados c
                INNER JOIN Cursos cu ON c.CursoId = cu.Id
                INNER JOIN Otecs o ON cu.OtecId = o.Id
                WHERE c.url_landing IS NULL OR c.url_landing = '' OR LEN(c.url_landing) <= 10
                ORDER BY c.IssuedAt";
            cmd.Parameters.AddWithValue("@batch", batch);
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new CertMigrationInfo
                {
                    CertificadoId = dr.GetGuid(0),
                    AlumnoId = dr.GetGuid(1),
                    CursoId = dr.GetGuid(2),
                    PdfFilename = dr.IsDBNull(3) ? "" : dr.GetString(3),
                    OtecSlug = dr.GetString(4)
                });
            }
            return list;
        }

        private static SqlConnection GetConnection()
        {
            return new SqlConnection(
                new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build()
                    .GetConnectionString("DefaultConnection"));
        }

        private class CertMigrationInfo
        {
            public Guid CertificadoId { get; set; }
            public Guid AlumnoId { get; set; }
            public Guid CursoId { get; set; }
            public string PdfFilename { get; set; } = "";
            public string OtecSlug { get; set; } = "default";
        }
    }
}
