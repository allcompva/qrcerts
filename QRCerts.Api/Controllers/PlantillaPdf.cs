namespace QRCerts.Api.Controllers
{
    public class PlantillaPdf
    {
        public PlantillaPdf()
        {
            Pages = new List<PdfPage>();
        }

        public List<PdfPage> Pages { get; set; }
    }
}
