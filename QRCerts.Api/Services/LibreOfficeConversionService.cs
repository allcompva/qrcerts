using System.Diagnostics;

namespace QRCerts.Api.Services
{
    /// <summary>
    /// Servicio para convertir documentos DOCX a PDF usando LibreOffice
    /// </summary>
    public interface ILibreOfficeConversionService
    {
        Task<byte[]> ConvertDocxToPdfAsync(byte[] docxBytes, string fileName = "document.docx");
        Task<byte[]> ConvertDocxToPdfAsync(string docxFilePath);
    }

    public class LibreOfficeConversionService : ILibreOfficeConversionService
    {
        private readonly ILogger<LibreOfficeConversionService> _logger;
        private readonly IConfiguration _configuration;

        public LibreOfficeConversionService(ILogger<LibreOfficeConversionService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<byte[]> ConvertDocxToPdfAsync(byte[] docxBytes, string fileName = "document.docx")
        {
            string tmpDir = Path.Combine(Path.GetTempPath(), "conv_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tmpDir);

            string docxPath = Path.Combine(tmpDir, SanitizeFileName(fileName));
            if (!docxPath.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
                docxPath += ".docx";

            try
            {
                await File.WriteAllBytesAsync(docxPath, docxBytes);
                return await ConvertDocxToPdfAsync(docxPath);
            }
            finally
            {
                // Limpieza
                CleanupDirectory(tmpDir);
            }
        }

        public async Task<byte[]> ConvertDocxToPdfAsync(string docxFilePath)
        {
            if (!File.Exists(docxFilePath))
                throw new FileNotFoundException($"Archivo no encontrado: {docxFilePath}");

            string tmpDir = Path.GetDirectoryName(docxFilePath) ?? Path.GetTempPath();
            string pdfPath = Path.Combine(tmpDir, Path.GetFileNameWithoutExtension(docxFilePath) + ".pdf");

            // Obtener configuración de LibreOffice
            var soffice = _configuration["LibreOffice:Path"]
                ?? Environment.GetEnvironmentVariable("SOFFICE_PATH")
                ?? GetDefaultLibreOfficePath();

            var timeoutMs = _configuration.GetValue<int>("LibreOffice:TimeoutMs", 120000);

            _logger.LogInformation("Convirtiendo {DocxPath} a PDF usando LibreOffice en {Soffice}", docxFilePath, soffice);

            var psi = new ProcessStartInfo
            {
                FileName = soffice,
                Arguments = $"--headless --nologo --nofirststartwizard --convert-to pdf --outdir \"{tmpDir}\" \"{docxFilePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            if (proc == null)
            {
                throw new InvalidOperationException("No se pudo iniciar el proceso de LibreOffice");
            }

            var outputTask = proc.StandardOutput.ReadToEndAsync();
            var errorTask = proc.StandardError.ReadToEndAsync();

            if (!proc.WaitForExit(timeoutMs))
            {
                try { proc.Kill(); } catch { }
                _logger.LogError("Timeout al convertir documento después de {Timeout}ms", timeoutMs);
                throw new TimeoutException("El proceso de conversión excedió el tiempo límite");
            }

            var output = await outputTask;
            var error = await errorTask;

            if (proc.ExitCode != 0)
            {
                _logger.LogError("Error en conversión. Exit code: {ExitCode}, StdErr: {StdErr}, StdOut: {StdOut}",
                    proc.ExitCode, error, output);
                throw new InvalidOperationException($"Error en la conversión: {error}");
            }

            // Esperar un momento para que el archivo se escriba completamente
            await Task.Delay(100);

            if (!File.Exists(pdfPath))
            {
                _logger.LogError("El archivo PDF no fue generado: {PdfPath}", pdfPath);
                throw new FileNotFoundException("El archivo PDF no fue generado", pdfPath);
            }

            var pdfBytes = await File.ReadAllBytesAsync(pdfPath);
            _logger.LogInformation("Conversión exitosa: {FileName}, tamaño: {Size} bytes",
                Path.GetFileName(pdfPath), pdfBytes.Length);

            return pdfBytes;
        }

        private string GetDefaultLibreOfficePath()
        {
            // En Docker/Linux
            if (File.Exists("/usr/bin/soffice"))
                return "/usr/bin/soffice";

            // En Docker/Linux alternativo
            if (File.Exists("/usr/bin/libreoffice"))
                return "/usr/bin/libreoffice";

            // En Windows - rutas comunes
            var windowsPaths = new[]
            {
                @"C:\Program Files\LibreOffice\program\soffice.exe",
                @"C:\Program Files (x86)\LibreOffice\program\soffice.exe",
                Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Programs\LibreOffice\program\soffice.exe")
            };

            foreach (var path in windowsPaths)
            {
                if (File.Exists(path))
                    return path;
            }

            // Fallback - asume que está en el PATH
            return "soffice";
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }

        private void CleanupDirectory(string dirPath)
        {
            try
            {
                if (Directory.Exists(dirPath))
                    Directory.Delete(dirPath, recursive: true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al limpiar directorio temporal: {Dir}", dirPath);
            }
        }
    }
}
