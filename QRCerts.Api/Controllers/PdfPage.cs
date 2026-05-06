namespace QRCerts.Api.Controllers
{
    public class PdfPage
    {
        public PdfPage()
        {
            Elements = new List<Element>();
        }

        /// <summary>
        /// "portrait" o "landscape"
        /// </summary>
        public string Orientation { get; set; } = "portrait";

        /// <summary>
        /// Imagen de fondo en base64 (opcional)
        /// </summary>
        public string BackgroundImageBase64 { get; set; }

        /// <summary>
        /// Elementos de la página: texto, imagen, header, footer
        /// </summary>
        public List<Element> Elements { get; set; }
    }
}
