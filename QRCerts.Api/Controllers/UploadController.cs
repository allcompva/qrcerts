using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using QRCerts.Api.Services;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using iTextSharp.text;
using QRCoder;
using RestSharp;
using System.Net.Mime;
using System.Text.Json;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using System.Diagnostics;
namespace QRCerts.Api.Controllers
{
    /// <summary>Maneja uploads de archivos (imágenes y documentos .docx)</summary>
    [ApiController]
    [Route("api/app/upload")]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IPlantillaCertificadosService _plantillaService;
        private readonly IPdfJobRepository _pdfJobRepo;
        private readonly IGoogleDriveService _driveService;
        private readonly string _qrBaseUrl;
        public UploadController(IPlantillaCertificadosService plantillaService,
            IPdfJobRepository pdfJobRepository, IWebHostEnvironment env,
            IGoogleDriveService driveService,
            IConfiguration configuration)
        {
            _plantillaService = plantillaService;
            _env = env;
            _pdfJobRepo = pdfJobRepository;
            _driveService = driveService;
            _qrBaseUrl = configuration["QrBaseUrl"] ?? "https://certificadosqr.store/app/#/validar?data=";
        }

        /// <summary>Sube una imagen y retorna el nombre del archivo guardado</summary>
        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No se proporcionó archivo" });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                return BadRequest(new { message = "Solo se permiten archivos de imagen" });

            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "images");
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { fileName });
        }

        /// <summary>Sube un archivo .docx, lo guarda y extrae variables en formato -VARIABLE-</summary>
        [HttpPost("docx")]
        public async Task<IActionResult> UploadDocx(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No se proporcionó archivo" });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".docx")
                return BadRequest(new { message = "Solo se permiten archivos .docx" });

            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "docx");
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}.docx";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Extraer variables del docx
            var variables = ExtractVariablesFromDocx(filePath);

            // Subir plantilla a Google Drive
            string? driveFileId = null;
            try
            {
                var otecIdClaim = User.FindFirst("otec_id")?.Value;
                var otecSlug = "default";
                if (!string.IsNullOrEmpty(otecIdClaim) && Guid.TryParse(otecIdClaim, out var otecGuid))
                {
                    var otec = DAL.OtecDAL.GetById(otecGuid);
                    if (otec != null) otecSlug = otec.Slug;
                }

                using var uploadStream = System.IO.File.OpenRead(filePath);
                driveFileId = await _driveService.UploadFileAsync(
                    uploadStream, fileName, $"Plantillas/{otecSlug}",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            }
            catch (Exception driveEx)
            {
                Console.WriteLine($"Error subiendo plantilla a Drive: {driveEx.Message}");
            }

            return Ok(new
            {
                docxPath = fileName,
                driveFileId = driveFileId,
                variables = variables.ToArray()
            });
        }

        /// <summary>
        /// Extrae variables en formato -VARIABLE- de un archivo .docx
        /// Los archivos .docx son archivos ZIP que contienen XML
        /// </summary>

        /// <summary>
        /// Extrae placeholders del tipo {{KEY}} (por defecto) desde todas las partes xml dentro de word/
        /// Reconstituye el texto por párrafo (concat de w:t) para que placeholders rotos por runs sean encontrados.
        /// </summary>
        /// <param name="docxPath">Ruta al .docx</param>
        /// <param name="pattern">Regex con un grupo capturador para la key (por defecto: \{\{([\p{L}0-9_]+)\}\})</param>
        /// <returns>Lista única de keys encontradas</returns>
        public static List<string> ExtractVariablesFromDocx(string docxPath, string pattern = @"\{\{([\p{L}0-9_]+)\}\}")
        {
            var variables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(docxPath))
                return variables.ToList();

            try
            {
                // Compilar regex para performance
                var regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);

                using (var archive = ZipFile.OpenRead(docxPath))
                {
                    // Tomamos todas las entradas xml dentro de word/ excepto rels y media binaria
                    var xmlEntries = archive.Entries
                        .Where(e =>
                            e.FullName.StartsWith("word/", StringComparison.OrdinalIgnoreCase)
                            && e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
                            && !e.FullName.StartsWith("word/_rels", StringComparison.OrdinalIgnoreCase)
                            && !e.FullName.StartsWith("word/media", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    foreach (var entry in xmlEntries)
                    {
                        using (var stream = entry.Open())
                        {
                            // Cargar xml con XDocument para navegar por nodos w:p y w:t
                            XDocument xdoc;
                            try
                            {
                                xdoc = XDocument.Load(stream);
                            }
                            catch
                            {
                                // si algún xml está corrupto o no es well-formed, lo ignoramos
                                continue;
                            }

                            // Namespace local indiferente — buscamos por LocalName
                            // Recorremos todos los párrafos <w:p>
                            var paragraphs = xdoc.Descendants().Where(x => x.Name.LocalName == "p");
                            foreach (var p in paragraphs)
                            {
                                // Reconstruimos el texto del párrafo: concatenamos todos los w:t en orden
                                var texts = p.Descendants()
                                             .Where(x => x.Name.LocalName == "t")
                                             .Select(x => (x.Value ?? string.Empty))
                                             .ToArray();

                                if (texts.Length == 0)
                                    continue;

                                var paragraphText = string.Concat(texts);

                                // Buscar placeholders en el párrafo reconstruido
                                var matches = regex.Matches(paragraphText);
                                foreach (Match m in matches)
                                {
                                    if (m.Success && m.Groups.Count > 1)
                                    {
                                        var key = m.Groups[1].Value.Trim();
                                        if (!string.IsNullOrWhiteSpace(key))
                                            variables.Add(key);
                                    }
                                }
                            }

                            // Además puede haber texto en elementos que no están dentro de w:p (por ejemplo algunos v:textbox)
                            // Para cubrir esos casos, también buscamos cualquier w:t aislado fuera de w:p y aplicamos regex sobre fragmentos cortos
                            var standaloneTs = xdoc.Descendants().Where(x => x.Name.LocalName == "t" && x.Ancestors().All(a => a.Name.LocalName != "p"));
                            foreach (var t in standaloneTs)
                            {
                                var txt = (t.Value ?? string.Empty);
                                var matches2 = regex.Matches(txt);
                                foreach (Match m in matches2)
                                {
                                    if (m.Success && m.Groups.Count > 1)
                                    {
                                        var key = m.Groups[1].Value.Trim();
                                        if (!string.IsNullOrWhiteSpace(key))
                                            variables.Add(key);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // No lanzamos excepción hacia arriba para no romper el flujo; loguear si querés.
                Console.WriteLine($"    extrayendo variables del docx: {ex.Message}");
            }

            return variables.ToList();
        }

        // Modelo de la request que espera JSON directo
        /// <summary>
        /// Reemplaza placeholders {{KEY}} dentro del DOCX incluso si están partidos en múltiples <w:t>.
        /// Crea un nuevo docx (outputPath) a partir del inputPath.
        /// </summary>
        /// <summary>
        /// Genera un PDF a partir de la plantilla del curso y los datos del alumno.
        /// Retorna el path del PDF generado. El caller es responsable de limpiar los archivos temporales.
        /// </summary>
        public static async Task<string?> GeneratePdfToFile(
            Guid courseGuid, Guid alumnoGuid,
            IPlantillaCertificadosService plantillaService,
            string webRootPath,
            string qrBaseUrl,
            CancellationToken ct)
        {
            string templateFileName = await plantillaService.GetTemplateFileNameAsync(courseGuid);
            if (string.IsNullOrWhiteSpace(templateFileName))
                return null;

            var plantillaPath = Path.Combine(webRootPath, "uploads", "docx", templateFileName);
            if (!System.IO.File.Exists(plantillaPath))
                return null;

            string jsonString = await plantillaService.GetReplacementJsonAsync(courseGuid, alumnoGuid);
            if (string.IsNullOrWhiteSpace(jsonString))
                jsonString = "{}";

            Dictionary<string, string> values;
            try
            {
                values = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString)
                         ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                values = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString)
                         ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            var normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in values)
            {
                if (kv.Key == null) continue;
                normalized[kv.Key.Trim()] = kv.Value ?? string.Empty;
            }

            var tempDocx = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".docx");
            var pdfOutDir = Path.Combine(Path.GetTempPath(), "pdfout_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(pdfOutDir);

            System.IO.File.Copy(plantillaPath, tempDocx, overwrite: true);

            var qrData = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{alumnoGuid},{courseGuid}"));
            var qrUrl = $"{qrBaseUrl}{qrData}";
            var qrImageBytes = GenerateQrCodePng(qrUrl, 150);

            ReplaceQrPlaceholderWithImage(tempDocx, qrImageBytes);
            ReplacePlaceholdersInDocx(tempDocx, tempDocx, normalized);

            var sofficePath = Environment.GetEnvironmentVariable("SOFFICE_PATH")
                              ?? (Environment.OSVersion.Platform == PlatformID.Win32NT
                                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "LibreOffice", "program", "soffice.exe")
                                    : "/usr/bin/soffice");

            var pdfPath = await LibreOfficeHelper.ConvertDocxToPdfWithLibreOfficeAsync(
                inputDocx: tempDocx,
                outputDir: pdfOutDir,
                sofficePath: sofficePath,
                timeoutMs: 120_000,
                cancellationToken: ct);

            // Limpiar docx temporal
            try { if (System.IO.File.Exists(tempDocx)) System.IO.File.Delete(tempDocx); } catch { }

            if (string.IsNullOrWhiteSpace(pdfPath) || !System.IO.File.Exists(pdfPath))
                return null;

            return pdfPath;
        }

        [HttpGet("generate-pdf-by-ids")]
        public async Task<IActionResult> GeneratePdfByIds([FromQuery] string courseId, [FromQuery] string alumnoId)
        {
            if (!Guid.TryParse(courseId, out var courseGuid))
                return BadRequest(new { message = "courseId inválido. Debe ser GUID." });

            if (!Guid.TryParse(alumnoId, out var alumnoGuid))
                return BadRequest(new { message = "alumnoId inválido. Debe ser GUID." });

            // Llamadas a la DAL (ajustá el objeto _dal a tu implementación)
            string templateFileName = await _plantillaService.GetTemplateFileNameAsync(courseGuid);
            if (string.IsNullOrWhiteSpace(templateFileName))
                return StatusCode(500, new { message = "No se encontró plantilla asociada al curso." });

            // ruta fija en wwwroot/uploads/docx
            var plantillaPath = Path.Combine(_env.WebRootPath ?? Directory.GetCurrentDirectory(), "uploads", "docx", templateFileName);
            if (!System.IO.File.Exists(plantillaPath))
                return StatusCode(500, new { message = $"Plantilla no encontrada: {plantillaPath}" });

            // Obtener JSON de la DAL
            string jsonString = await _plantillaService.GetReplacementJsonAsync(courseGuid, alumnoGuid);
            if (string.IsNullOrWhiteSpace(jsonString))
                jsonString = "{}";

            // Mapear JSON -> Dictionary<string,string>
            Dictionary<string, string> values;
            try
            {
                values = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString)
                         ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            }
            catch
            {
                values = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString)
                         ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            // Normalizar claves (case-insensitive)
            var normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in values)
            {
                if (kv.Key == null) continue;
                normalized[kv.Key.Trim()] = kv.Value ?? string.Empty;
            }

            // Temp paths
            var tempDocx = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".docx");
            var pdfOutDir = Path.Combine(Path.GetTempPath(), "pdfout_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(pdfOutDir);

            try
            {
                System.IO.File.Copy(plantillaPath, tempDocx, overwrite: true);

                // Generar QR con los datos del certificado (igual que en los otros flujos)
                var qrData = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{alumnoId},{courseId}"));
                var qrUrl = $"{_qrBaseUrl}{qrData}";
                var qrImageBytes = GenerateQrCodePng(qrUrl, 150);

                // Reemplazar {{QR}} con la imagen del QR en el documento
                ReplaceQrPlaceholderWithImage(tempDocx, qrImageBytes);

                // Reemplazo robusto de texto (maneja runs partidos)
                ReplacePlaceholdersInDocx(tempDocx, tempDocx, normalized);

                // Ruta de LibreOffice configurable por variable de entorno o auto-detectada
                var sofficePath = Environment.GetEnvironmentVariable("SOFFICE_PATH")
                                  ?? (Environment.OSVersion.Platform == PlatformID.Win32NT
                                        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "LibreOffice", "program", "soffice.exe")
                                        : "/usr/bin/soffice");
                int timeoutMs = 120_000; // 2 min, ajustá si necesitás más

                string pdfPath;
                try
                {
                    pdfPath = await LibreOfficeHelper.ConvertDocxToPdfWithLibreOfficeAsync(
                        inputDocx: tempDocx,
                        outputDir: pdfOutDir,
                        sofficePath: sofficePath,
                        timeoutMs: timeoutMs,
                        cancellationToken: HttpContext.RequestAborted
                    );
                }
                catch (OperationCanceledException)
                {
                    return StatusCode(499, new { message = "Solicitud cancelada por el cliente." });
                }
                catch (TimeoutException tex)
                {
                    return StatusCode(504, new { message = $"Timeout en conversión: {tex.Message}" });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = $"Error convirtiendo docx a pdf: {ex.Message}" });
                }

                if (string.IsNullOrWhiteSpace(pdfPath) || !System.IO.File.Exists(pdfPath))
                {
                    return StatusCode(500, new { message = "No se generó el PDF." });
                }

                // Devolver el PDF generado
                var downloadName = Path.GetFileNameWithoutExtension(plantillaPath) + ".pdf";
                var resultStream = System.IO.File.OpenRead(pdfPath);
                HttpContext.Response.OnCompleted(() =>
                {
                    try
                    {
                        resultStream.Dispose();
                        if (System.IO.File.Exists(pdfPath)) System.IO.File.Delete(pdfPath);
                    }
                    catch { }
                    return Task.CompletedTask;
                });

                return File(resultStream, "application/pdf", downloadName);


            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            finally
            {
                // limpieza silenciosa
                try { if (System.IO.File.Exists(tempDocx)) System.IO.File.Delete(tempDocx); } catch { }
                try
                {
                    if (Directory.Exists(pdfOutDir))
                    {
                        foreach (var f in Directory.GetFiles(pdfOutDir)) { try { System.IO.File.Delete(f); } catch { } }
                        Directory.Delete(pdfOutDir, recursive: true);
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Convierte DOCX a PDF usando LibreOffice (soffice) en modo headless.
        /// outputDir debe existir.
        /// </summary>


        public static void ReplacePlaceholdersInDocx(string inputDocxPath, string outputDocxPath, Dictionary<string, string> values, string placeholderPattern = @"\{\{([\p{L}0-9_]+)\}\}")
        {
            if (!string.Equals(inputDocxPath, outputDocxPath, StringComparison.OrdinalIgnoreCase))
                System.IO.File.Copy(inputDocxPath, outputDocxPath, overwrite: true);

            using (var archive = ZipFile.Open(outputDocxPath, ZipArchiveMode.Update))
            {
                var xmlEntries = archive.Entries
                    .Where(e => e.FullName.StartsWith("word/") && e.FullName.EndsWith(".xml") && !e.FullName.Contains("media") && !e.FullName.Contains("_rels"))
                    .ToList();

                var regex = new Regex(placeholderPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);

                foreach (var entry in xmlEntries)
                {
                    XDocument xdoc;
                    using (var s = entry.Open()) { try { xdoc = XDocument.Load(s); } catch { continue; } }

                    bool modified = false;

                    // Procesar cada párrafo (w:p) por separado
                    var paragraphs = xdoc.Descendants().Where(x => x.Name.LocalName == "p").ToList();

                    foreach (var paragraph in paragraphs)
                    {
                        modified |= ReplaceInTextNodes(paragraph, regex, values);
                    }

                    // También procesar textboxes y otros contenedores que no estén dentro de párrafos
                    var standaloneContainers = xdoc.Descendants()
                        .Where(x => x.Name.LocalName == "txbxContent" ||
                                   (x.Name.LocalName == "t" && !x.Ancestors().Any(a => a.Name.LocalName == "p")))
                        .ToList();

                    foreach (var container in standaloneContainers)
                    {
                        if (container.Name.LocalName == "t")
                        {
                            // Nodo de texto individual
                            var originalText = container.Value ?? string.Empty;
                            if (regex.IsMatch(originalText))
                            {
                                container.Value = regex.Replace(originalText, m =>
                                {
                                    var key = m.Groups[1].Value;
                                    if (values.TryGetValue(key, out var val)) return val ?? "";
                                    var kv = values.FirstOrDefault(k => string.Equals(k.Key, key, StringComparison.OrdinalIgnoreCase));
                                    return kv.Key != null ? kv.Value ?? "" : "";
                                });
                                modified = true;
                            }
                        }
                        else
                        {
                            // Contenedor de textbox - procesar sus párrafos
                            var innerParagraphs = container.Descendants().Where(x => x.Name.LocalName == "p").ToList();
                            foreach (var innerP in innerParagraphs)
                            {
                                modified |= ReplaceInTextNodes(innerP, regex, values);
                            }
                        }
                    }

                    if (modified)
                    {
                        entry.Delete();
                        var newEntry = archive.CreateEntry(entry.FullName);
                        using (var writer = new StreamWriter(newEntry.Open(), Encoding.UTF8)) xdoc.Save(writer);
                    }
                }
            }
        }

        /// <summary>
        /// Reemplaza placeholders en los nodos de texto de un contenedor (párrafo).
        /// Maneja el caso donde el placeholder está partido en múltiples runs.
        /// </summary>
        private static bool ReplaceInTextNodes(XElement container, Regex regex, Dictionary<string, string> values)
        {
            var textNodes = container.Descendants().Where(x => x.Name.LocalName == "t").ToList();
            if (!textNodes.Any()) return false;

            // Construir mapa de posiciones: para cada carácter del texto concatenado,
            // saber a qué nodo pertenece y en qué posición dentro del nodo
            var nodeMap = new List<(XElement node, int indexInNode)>();
            foreach (var node in textNodes)
            {
                var text = node.Value ?? string.Empty;
                for (int i = 0; i < text.Length; i++)
                {
                    nodeMap.Add((node, i));
                }
            }

            var fullText = string.Concat(textNodes.Select(t => t.Value ?? string.Empty));
            var matches = regex.Matches(fullText);
            if (matches.Count == 0) return false;

            // Procesar matches de atrás hacia adelante para no invalidar índices
            var matchList = matches.Cast<Match>().OrderByDescending(m => m.Index).ToList();

            foreach (var match in matchList)
            {
                var key = match.Groups[1].Value;
                string replacement;
                if (values.TryGetValue(key, out var val))
                    replacement = val ?? "";
                else
                {
                    var kv = values.FirstOrDefault(k => string.Equals(k.Key, key, StringComparison.OrdinalIgnoreCase));
                    replacement = kv.Key != null ? kv.Value ?? "" : "";
                }

                int startIdx = match.Index;
                int endIdx = match.Index + match.Length - 1;

                if (startIdx >= nodeMap.Count) continue;
                if (endIdx >= nodeMap.Count) endIdx = nodeMap.Count - 1;

                var startNode = nodeMap[startIdx].node;
                var startPosInNode = nodeMap[startIdx].indexInNode;
                var endNode = nodeMap[endIdx].node;
                var endPosInNode = nodeMap[endIdx].indexInNode;

                if (startNode == endNode)
                {
                    // El placeholder está completamente en un solo nodo
                    var nodeText = startNode.Value ?? "";
                    var before = nodeText.Substring(0, startPosInNode);
                    var after = endPosInNode + 1 < nodeText.Length ? nodeText.Substring(endPosInNode + 1) : "";
                    startNode.Value = before + replacement + after;
                }
                else
                {
                    // El placeholder está partido en múltiples nodos
                    // 1. En el nodo inicial: mantener texto antes del placeholder + poner el replacement
                    var startText = startNode.Value ?? "";
                    var before = startText.Substring(0, startPosInNode);
                    startNode.Value = before + replacement;

                    // 2. En los nodos intermedios: vaciarlos
                    bool inRange = false;
                    foreach (var node in textNodes)
                    {
                        if (node == startNode) { inRange = true; continue; }
                        if (node == endNode) break;
                        if (inRange) node.Value = "";
                    }

                    // 3. En el nodo final: mantener solo el texto después del placeholder
                    var endText = endNode.Value ?? "";
                    var after = endPosInNode + 1 < endText.Length ? endText.Substring(endPosInNode + 1) : "";
                    endNode.Value = after;
                }
            }

            return true;
        }

        /// <summary>
        /// Genera un código QR como imagen PNG en bytes
        /// </summary>
        public static byte[] GenerateQrCodePng(string content, int pixelsPerModule = 10)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new PngByteQRCode(qrCodeData))
                {
                    return qrCode.GetGraphic(pixelsPerModule);
                }
            }
        }

        /// <summary>
        /// Reemplaza el placeholder {{QR}} con una imagen QR en el documento Word
        /// </summary>
        public static void ReplaceQrPlaceholderWithImage(string docxPath, byte[] imageBytes, int widthCm = 3, int heightCm = 3)
        {
            using (var doc = WordprocessingDocument.Open(docxPath, true))
            {
                var mainPart = doc.MainDocumentPart;
                if (mainPart == null) return;

                // Agregar la imagen como parte del documento
                var imagePart = mainPart.AddImagePart(ImagePartType.Png);
                using (var stream = new MemoryStream(imageBytes))
                {
                    imagePart.FeedData(stream);
                }

                string relationshipId = mainPart.GetIdOfPart(imagePart);

                // Convertir cm a EMUs (English Metric Units) - 1 cm = 360000 EMUs
                long widthEmu = widthCm * 360000;
                long heightEmu = heightCm * 360000;

                // Buscar y reemplazar {{QR}} en el documento
                var body = mainPart.Document.Body;
                if (body == null) return;

                // Buscar todos los textos que contengan {{QR}}
                var textsWithQr = body.Descendants<Text>()
                    .Where(t => t.Text != null && t.Text.Contains("{{QR}}"))
                    .ToList();

                foreach (var textElement in textsWithQr)
                {
                    // Obtener el Run padre
                    var parentRun = textElement.Parent as Run;
                    if (parentRun == null) continue;

                    // Si el texto es exactamente {{QR}}, reemplazar todo el Run con la imagen
                    if (textElement.Text.Trim() == "{{QR}}")
                    {
                        var drawing = CreateImageDrawing(relationshipId, widthEmu, heightEmu);
                        parentRun.RemoveAllChildren<Text>();
                        parentRun.AppendChild(drawing);
                    }
                    else
                    {
                        // Si {{QR}} está mezclado con otro texto, separar
                        var parts = textElement.Text.Split(new[] { "{{QR}}" }, StringSplitOptions.None);
                        if (parts.Length > 1)
                        {
                            var paragraph = parentRun.Parent as Paragraph;
                            if (paragraph == null) continue;

                            // Crear runs para cada parte
                            var runsBefore = new List<OpenXmlElement>();

                            for (int i = 0; i < parts.Length; i++)
                            {
                                if (!string.IsNullOrEmpty(parts[i]))
                                {
                                    var newRun = parentRun.CloneNode(true) as Run;
                                    var newText = newRun?.Descendants<Text>().FirstOrDefault();
                                    if (newText != null) newText.Text = parts[i];
                                    if (newRun != null) runsBefore.Add(newRun);
                                }

                                // Agregar imagen entre partes (excepto después de la última)
                                if (i < parts.Length - 1)
                                {
                                    var imageRun = new Run(CreateImageDrawing(relationshipId, widthEmu, heightEmu));
                                    runsBefore.Add(imageRun);
                                }
                            }

                            // Insertar los nuevos runs y eliminar el original
                            foreach (var newRun in runsBefore)
                            {
                                paragraph.InsertBefore(newRun, parentRun);
                            }
                            parentRun.Remove();
                        }
                    }
                }

                // También buscar en headers y footers
                foreach (var headerPart in mainPart.HeaderParts)
                {
                    ReplaceQrInPart(headerPart, imagePart, widthEmu, heightEmu);
                }
                foreach (var footerPart in mainPart.FooterParts)
                {
                    ReplaceQrInPart(footerPart, imagePart, widthEmu, heightEmu);
                }

                doc.Save();
            }
        }

        private static void ReplaceQrInPart(OpenXmlPart part, ImagePart imagePart, long widthEmu, long heightEmu, string placeholder = "{{QR}}")
        {
            if (part == null) throw new ArgumentNullException(nameof(part));
            if (imagePart == null) throw new ArgumentNullException(nameof(imagePart));

            // 1) Asegurar que el imagePart esté en 'part'. Si no, copiar su contenido a un nuevo ImagePart del mismo part.
            string relId;
            try
            {
                relId = part.GetIdOfPart(imagePart); // puede lanzar ArgumentOutOfRangeException si no pertenece
            }
            catch (ArgumentOutOfRangeException)
            {
                // Copiar contenido del imagePart externo a uno nuevo en 'part'
                // Usamos dynamic porque AddImagePart es un método de extensión que requiere tipos específicos
                ImagePart newImagePart;
                var imageType = imagePart.ContentType switch
                {
                    "image/png" => ImagePartType.Png,
                    "image/jpeg" => ImagePartType.Jpeg,
                    _ => ImagePartType.Png
                };

                if (part is HeaderPart headerPart)
                    newImagePart = headerPart.AddImagePart(imageType);
                else if (part is FooterPart footerPart)
                    newImagePart = footerPart.AddImagePart(imageType);
                else
                    throw new NotSupportedException($"El tipo de parte {part.GetType().Name} no soporta AddImagePart");

                using (var src = imagePart.GetStream())
                using (var dst = newImagePart.GetStream(FileMode.Create, FileAccess.Write))
                {
                    src.CopyTo(dst);
                }

                relId = part.GetIdOfPart(newImagePart);
            }

            // 2) Crear drawing que referencia relId
            var drawing = CreateImageDrawing(relId, widthEmu, heightEmu);

            // 3) Reemplazo robusto del placeholder (maneja runs partidos)
            var root = part.RootElement;
            if (root == null) return;

            var texts = root.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>().ToList();
            if (!texts.Any()) return;

            for (int i = 0; i < texts.Count; i++)
            {
                var sb = new System.Text.StringBuilder();
                int j = i;
                while (j < texts.Count && sb.Length <= 5000)
                {
                    sb.Append(texts[j].Text ?? string.Empty);
                    var combined = sb.ToString();
                    int idx = combined.IndexOf(placeholder, StringComparison.Ordinal);
                    if (idx >= 0)
                    {
                        string prefix = combined.Substring(0, idx);
                        string suffix = combined.Substring(idx + placeholder.Length);

                        texts[i].Text = prefix;
                        for (int k = i + 1; k <= j - 1; k++) texts[k].Text = string.Empty;
                        texts[j].Text = suffix;

                        var runAfter = texts[j].Ancestors<DocumentFormat.OpenXml.Wordprocessing.Run>().FirstOrDefault()
                                       ?? texts[i].Ancestors<DocumentFormat.OpenXml.Wordprocessing.Run>().FirstOrDefault();

                        if (runAfter != null)
                        {
                            var newRun = new DocumentFormat.OpenXml.Wordprocessing.Run(drawing.CloneNode(true));
                            runAfter.Parent.InsertAfter(newRun, runAfter);
                        }
                        else
                        {
                            var parentOfText = texts[i].Parent;
                            if (parentOfText != null)
                            {
                                var newRun = new DocumentFormat.OpenXml.Wordprocessing.Run(drawing.CloneNode(true));
                                parentOfText.AppendChild(newRun);
                            }
                        }

                        i = j;
                        break;
                    }

                    j++;
                }
            }

            root.Save();
        }


        /// <summary>
        /// Crea el elemento Drawing para insertar una imagen en Word
        /// </summary>
        private static Drawing CreateImageDrawing(string relationshipId, long widthEmu, long heightEmu)
        {
            var element = new Drawing(
                new DW.Inline(
                    new DW.Extent() { Cx = widthEmu, Cy = heightEmu },
                    new DW.EffectExtent()
                    {
                        LeftEdge = 0L,
                        TopEdge = 0L,
                        RightEdge = 0L,
                        BottomEdge = 0L
                    },
                    new DW.DocProperties()
                    {
                        Id = (UInt32Value)1U,
                        Name = "QR Code"
                    },
                    new DW.NonVisualGraphicFrameDrawingProperties(
                        new A.GraphicFrameLocks() { NoChangeAspect = true }),
                    new A.Graphic(
                        new A.GraphicData(
                            new PIC.Picture(
                                new PIC.NonVisualPictureProperties(
                                    new PIC.NonVisualDrawingProperties()
                                    {
                                        Id = (UInt32Value)0U,
                                        Name = "qrcode.png"
                                    },
                                    new PIC.NonVisualPictureDrawingProperties()),
                                new PIC.BlipFill(
                                    new A.Blip()
                                    {
                                        Embed = relationshipId,
                                        CompressionState = A.BlipCompressionValues.Print
                                    },
                                    new A.Stretch(
                                        new A.FillRectangle())),
                                new PIC.ShapeProperties(
                                    new A.Transform2D(
                                        new A.Offset() { X = 0L, Y = 0L },
                                        new A.Extents() { Cx = widthEmu, Cy = heightEmu }),
                                    new A.PresetGeometry(
                                        new A.AdjustValueList()
                                    )
                                    { Preset = A.ShapeTypeValues.Rectangle }))
                        )
                        { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                )
                {
                    DistanceFromTop = (UInt32Value)0U,
                    DistanceFromBottom = (UInt32Value)0U,
                    DistanceFromLeft = (UInt32Value)0U,
                    DistanceFromRight = (UInt32Value)0U
                });

            return element;
        }


        public class BatchRequest
        {
            public string CourseId { get; set; }
            public string AlumnosCsv { get; set; } // o List<string> Alumnos
        }


        [HttpPost("generate-zip-by-ids")]
        public async Task<IActionResult> GenerateZipByIds(BatchRequest req)
        {
            var otecId = Guid.Parse(User.FindFirst("otec_id")!.Value);

            var job = new PdfJob
            {
                OtecId = otecId,
                CourseId = Guid.Parse(req.CourseId),
                AlumnoIdsCsv = req.AlumnosCsv,
                TipoJob = 0,
                Prioridad = 100
            };

            var jobId = await _pdfJobRepo.EnqueueAsync(job);
            return Ok(new { jobId });
        }
        [HttpGet("zip/status/{jobId}")]
        public async Task<IActionResult> GetZipStatus(Guid jobId)
        {
            var job = await _pdfJobRepo.GetByIdAsync(jobId);
            if (job == null) return NotFound();

            return Ok(new
            {
                job.Id,
                job.Estado,
                job.Error,
                ready = job.Estado == 2
            });
        }
        [HttpGet("zip/download/{jobId}")]
        public async Task<IActionResult> DownloadZip(Guid jobId)
        {
            var job = await _pdfJobRepo.GetByIdAsync(jobId);
            if (job == null || job.Estado != 2)
                return BadRequest("ZIP no listo");

            var stream = System.IO.File.OpenRead(job.OutputPath!);
            return File(stream, "application/zip", "certificados.zip");
        }

    }
}
