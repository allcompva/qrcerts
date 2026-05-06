using QRCerts.Api.Models;
using System.Diagnostics;

namespace QRCerts.Api.Services
{
    public class ConversionService:IConversionService
    {
        private readonly ILogger<ConversionService> _logger;
        private readonly IConfiguration _configuration;

        public ConversionService(ILogger<ConversionService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<ConvertResponse> ConvertToBase64Async(ConvertRequest request)
        {
            var (pdfBytes, fileName) = await ConvertDocxToPdfAsync(request);
            return new ConvertResponse
            {
                FileName = fileName,
                ContentBase64 = Convert.ToBase64String(pdfBytes)
            };
        }

        public async Task<(byte[] PdfBytes, string FileName)> ConvertToStreamAsync(ConvertRequest request)
        {
            return await ConvertDocxToPdfAsync(request);
        }

        private async Task<(byte[] PdfBytes, string FileName)> ConvertDocxToPdfAsync(ConvertRequest request)
        {
            string tmpDir = Path.Combine(Path.GetTempPath(), "conv_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tmpDir);

            string docxPath = Path.Combine(tmpDir, SanitizeFileName(request.FileName ?? "file.docx"));
            if (!docxPath.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
                docxPath += ".docx";

            string pdfPath = Path.Combine(tmpDir, Path.GetFileNameWithoutExtension(docxPath) + ".pdf");

            try
            {
                // Guardar el DOCX
                var docxBytes = Convert.FromBase64String(request.ContentBase64);
                await File.WriteAllBytesAsync(docxPath, docxBytes);

                // Obtener configuración
                var soffice = _configuration["Conversion:LibreOfficePath"]
                    ?? Environment.GetEnvironmentVariable("SOFFICE_PATH")
                    ?? @"C:\Users\Administrador\Downloads\LibreOfficePortable\App\libreoffice\program\soffice.exe";

                var timeoutMs = _configuration.GetValue<int>("Conversion:TimeoutMs", 60000);
                var tmoEnv = Environment.GetEnvironmentVariable("CONVERT_TIMEOUT_MS");
                if (int.TryParse(tmoEnv, out var tmoVal))
                    timeoutMs = tmoVal;

                _logger.LogInformation("Convirtiendo {DocxPath} a PDF usando {Soffice}", docxPath, soffice);

                var psi = new ProcessStartInfo
                {
                    FileName = soffice,
                    Arguments = $"--headless --nologo --convert-to pdf --outdir \"{tmpDir}\" \"{docxPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var proc = Process.Start(psi)!;
                if (!proc.WaitForExit(timeoutMs))
                {
                    try { proc.Kill(); } catch { }
                    _logger.LogError("Timeout al convertir documento");
                    throw new TimeoutException("El proceso de conversión excedió el tiempo límite");
                }

                if (proc.ExitCode != 0)
                {
                    var err = await proc.StandardError.ReadToEndAsync();
                    var outp = await proc.StandardOutput.ReadToEndAsync();
                    _logger.LogError("Error en conversión. Exit code: {ExitCode}, StdErr: {StdErr}, StdOut: {StdOut}",
                        proc.ExitCode, err, outp);
                    throw new InvalidOperationException($"Error en la conversión: {err}");
                }

                if (!File.Exists(pdfPath))
                {
                    _logger.LogError("El archivo PDF no fue generado: {PdfPath}", pdfPath);
                    throw new FileNotFoundException("El archivo PDF no fue generado");
                }

                var pdfBytes = await File.ReadAllBytesAsync(pdfPath);
                var fileName = Path.GetFileName(pdfPath);

                _logger.LogInformation("Conversión exitosa: {FileName}", fileName);

                return (pdfBytes, fileName);
            }
            finally
            {
                // Limpieza
                try
                {
                    if (Directory.Exists(tmpDir))
                        Directory.Delete(tmpDir, recursive: true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al limpiar directorio temporal: {TmpDir}", tmpDir);
                }
            }
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}
