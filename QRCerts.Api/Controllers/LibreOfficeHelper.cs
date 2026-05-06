namespace QRCerts.Api.Controllers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public static class LibreOfficeHelper
    {
        public static async Task<string> ConvertDocxToPdfWithLibreOfficeAsync(
            string inputDocx,
            string outputDir,
            string sofficePath,
            int timeoutMs = 60000,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(inputDocx)) throw new ArgumentNullException(nameof(inputDocx));
            if (string.IsNullOrWhiteSpace(outputDir)) throw new ArgumentNullException(nameof(outputDir));
            if (string.IsNullOrWhiteSpace(sofficePath)) throw new ArgumentNullException(nameof(sofficePath));
            if (!File.Exists(inputDocx)) throw new FileNotFoundException("No existe el DOCX de entrada.", inputDocx);

            Directory.CreateDirectory(outputDir);

            var expectedPdfName = Path.GetFileNameWithoutExtension(inputDocx) + ".pdf";
            var expectedPdf = Path.Combine(outputDir, expectedPdfName);

            // perfil temporal
            var userProfileDir = Path.Combine(Path.GetTempPath(), "libreoffice_profile_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(userProfileDir);

            var profileUri = userProfileDir.Replace("\\", "/");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                profileUri = "file:///" + profileUri; // un guion en -env y triple slash en el valor
            else
                profileUri = "file://" + profileUri;

            var args = $"-env:UserInstallation={profileUri} --headless --nologo --nofirststartwizard --norestore --convert-to pdf:writer_pdf_Export --outdir \"{outputDir}\" \"{inputDocx}\"";

            var psi = new ProcessStartInfo
            {
                FileName = sofficePath,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = outputDir
            };


            // Forzar variables por si LibreOffice intenta escribir en APPDATA
            psi.EnvironmentVariables["HOME"] = userProfileDir;
            psi.EnvironmentVariables["USERPROFILE"] = userProfileDir;
            psi.EnvironmentVariables["APPDATA"] = userProfileDir;

            var stdOut = new StringBuilder();
            var stdErr = new StringBuilder();

            using var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
            proc.OutputDataReceived += (s, e) => { if (e.Data != null) stdOut.AppendLine(e.Data); };
            proc.ErrorDataReceived += (s, e) => { if (e.Data != null) stdErr.AppendLine(e.Data); };

            // Matar procesos residuales opcional (útil en tests)
            try
            {
                foreach (var p in Process.GetProcessesByName("soffice.bin")) { try { p.Kill(); } catch { } }
            }
            catch { /* ignore */ }

            try
            {
                if (!proc.Start()) throw new InvalidOperationException("No se pudo iniciar el proceso soffice.");

                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                // Esperar a que el PDF aparezca o hasta timeout
                var sw = Stopwatch.StartNew();
                var pollIntervalMs = 500;

                while (!cancellationToken.IsCancellationRequested && sw.ElapsedMilliseconds < timeoutMs)
                {
                    // si el padre murió con código distinto de 0, capturamos y fallamos temprano
                    if (proc.HasExited && proc.ExitCode != 0)
                    {
                        // leemos buffers actuales
                        var err = stdErr.ToString();
                        var outp = stdOut.ToString();
                        throw new InvalidOperationException($"LibreOffice padre falló (exit {proc.ExitCode}). stderr: {err}\nstdout: {outp}");
                    }

                    // si el PDF ya existe, retornamos (éxito)
                    if (File.Exists(expectedPdf))
                    {
                        // opcional: esperar a que no queden soffice.bin activos (evita race)
                        var sw2 = Stopwatch.StartNew();
                        while (sw2.ElapsedMilliseconds < 5000) // short wait for child finish
                        {
                            if (!Process.GetProcessesByName("soffice.bin").Any()) break;
                            await Task.Delay(200, CancellationToken.None);
                        }

                        // escribir logs para debug
                        File.WriteAllText(Path.Combine(outputDir, "soffice_stdout.log"), stdOut.ToString());
                        File.WriteAllText(Path.Combine(outputDir, "soffice_stderr.log"), stdErr.ToString());

                        return expectedPdf;
                    }

                    // si el padre terminó pero el PDF no existe aún, esperar a los soffice.bin
                    if (proc.HasExited)
                    {
                        // esperar a que no queden soffice.bin o hasta timeout
                        if (!Process.GetProcessesByName("soffice.bin").Any())
                        {
                            // ninguno activo y no hay pdf -> falló
                            break;
                        }
                    }

                    await Task.Delay(pollIntervalMs, CancellationToken.None);
                }

                // Timeout o cancelación
                if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(cancellationToken);

                // Intentar matar proceso padre e hijos (limpieza)
                try
                {
                    if (!proc.HasExited) proc.Kill(entireProcessTree: true);
                }
                catch { /* ignore */ }

                // también intentar matar soffice.bin si quedó colgado
                foreach (var p in Process.GetProcessesByName("soffice.bin")) { try { p.Kill(); } catch { } }

                // volcamos logs
                File.WriteAllText(Path.Combine(outputDir, "soffice_stdout.log"), stdOut.ToString());
                File.WriteAllText(Path.Combine(outputDir, "soffice_stderr.log"), stdErr.ToString());
                File.WriteAllText(Path.Combine(outputDir, "soffice_failure_note.txt"),
                    $"Timeout después de {timeoutMs}ms. stdout len={stdOut.Length}, stderr len={stdErr.Length}");

                // buscar cualquier PDF alternativo
                var matches = Directory.GetFiles(outputDir, Path.GetFileNameWithoutExtension(inputDocx) + "*.pdf");
                if (matches.Length > 0) return matches[0];

                throw new TimeoutException($"LibreOffice no generó el PDF en {timeoutMs} ms. Revisa {Path.Combine(outputDir, "soffice_stderr.log")}");
            }
            finally
            {
                // opcional: no borramos userProfileDir para debug; podés eliminarlo si querés
            }
        }
    }

}
