namespace QRCerts.Api.Controllers
{
    public class Element
    {
        /// <summary>
        /// "text", "image", "header", "footer"
        /// </summary>
        public string Type { get; set; }

        public string Value { get; set; }

        public string Font { get; set; }
        public double FontSize { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public string Color { get; set; } = "#000000";

        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        /// <summary>
        /// "left", "center", "right"
        /// </summary>
        public string Align { get; set; } = "left";
    }
}
