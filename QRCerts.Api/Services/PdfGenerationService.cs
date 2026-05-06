using Microsoft.AspNetCore.Mvc;
using QRCerts.Api.Controllers;
using static QRCerts.Api.Controllers.UploadController;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using System.Diagnostics;
using System.IO.Compression;

namespace QRCerts.Api.Services
{
    public class PdfGenerationService : IPdfGenerationService
    {
        private readonly IPlantillaCertificadosService _plantilla;
        private readonly IWebHostEnvironment _env;

        public PdfGenerationService(
            IPlantillaCertificadosService plantilla,
            IWebHostEnvironment env)
        {
            _plantilla = plantilla;
            _env = env;
        }

        public async Task<string> GenerateZipAsync(
            Guid courseId,
            string alumnosCsv,
            CancellationToken ct)
        {
            var alumnos = alumnosCsv.Split(',')
                .Select(x => x.Trim())
                .Where(x => Guid.TryParse(x, out _))
                .Distinct()
                .ToList();

            if (!alumnos.Any())
                throw new Exception("Sin alumnos válidos");

            var template = await _plantilla.GetTemplateFileNameAsync(courseId);
            var plantillaPath = Path.Combine(
                _env.WebRootPath ?? Directory.GetCurrentDirectory(),
                "uploads", "docx", template);

            var soffice = Environment.GetEnvironmentVariable("SOFFICE_PATH")
                ?? (OperatingSystem.IsWindows()
                    ? Path.Combine(Environment.GetFolderPath(
                        Environment.SpecialFolder.ProgramFiles),
                        "LibreOffice", "program", "soffice.exe")
                    : "/usr/bin/soffice");

            var workDir = Path.Combine(Path.GetTempPath(), "cert_" + Guid.NewGuid());
            Directory.CreateDirectory(workDir);

            var pdfs = new List<string>();

            try
            {
                foreach (var a in alumnos)
                {
                    ct.ThrowIfCancellationRequested();

                    var alumnoId = Guid.Parse(a);
                    var json = await _plantilla
                        .GetReplacementJsonAsync(courseId, alumnoId) ?? "{}";

                    var data = System.Text.Json.JsonSerializer
                        .Deserialize<Dictionary<string, string>>(json)
                        ?? new Dictionary<string, string>();

                    var docx = Path.Combine(workDir, $"{alumnoId}.docx");
                    var pdfDir = Path.Combine(workDir, "pdf");
                    Directory.CreateDirectory(pdfDir);

                    File.Copy(plantillaPath, docx, true);

                    ReplacePlaceholdersInDocx(docx, docx, data);

                    var pdf = await LibreOfficeHelper
                        .ConvertDocxToPdfWithLibreOfficeAsync(
                            docx, pdfDir, soffice, 120_000, ct);

                    pdfs.Add(pdf);
                }

                var zipPath = Path.Combine(
                    Path.GetTempPath(),
                    $"certificados_{courseId}_{Guid.NewGuid()}.zip");

                using var zip = File.Create(zipPath);
                using var archive = new ZipArchive(zip, ZipArchiveMode.Create);

                foreach (var pdf in pdfs)
                {
                    var entry = archive.CreateEntry(
                        Path.GetFileName(pdf), CompressionLevel.Optimal);
                    using var es = entry.Open();
                    using var fs = File.OpenRead(pdf);
                    await fs.CopyToAsync(es, ct);
                }

                return zipPath;
            }
            finally
            {
                try { Directory.Delete(workDir, true); } catch { }
            }
        }
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
    }
}
