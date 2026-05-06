// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Controllers.ValidacionCertificadoResponse
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

#nullable enable
namespace QRCerts.Api.Controllers
{
    public class ValidacionCertificadoResponse
    {
        public bool EsValido { get; set; }

        public AlumnoValidacionDto? Alumno { get; set; }

        public CursoValidacionDto? Curso { get; set; }

        public string? Mensaje { get; set; }

        public string? PdfFilename { get; set; }
        public string vencimiento { get; set; }
    }
}
