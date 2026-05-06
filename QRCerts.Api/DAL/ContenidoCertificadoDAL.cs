using Microsoft.Data.SqlClient;
using System.Data;

namespace QRCerts.Api.DAL
{
    public class ContenidoCertificadoDAL : DALBase
    {
        public Guid Id { get; set; }
        public Guid IdCertificado { get; set; }
        public string NombreCurso { get; set; } = string.Empty;
        public string NombreAlumno { get; set; } = string.Empty;
        public string RUT { get; set; } = string.Empty;
        public string CertificateType { get; set; } = string.Empty;
        public string Footer1 { get; set; } = string.Empty;
        public string Footer2 { get; set; } = string.Empty;
        public string ContenidoHtml { get; set; } = string.Empty;
        public string FooterHtml { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string NombreReferenciaCurso { get; set; } = string.Empty;
        public string ImagenFondo { get; set; } = string.Empty;

        public static ContenidoCertificadoDAL? GetByCertificadoId(Guid certificadoId)
        {
            try
            {
                using var con = GetConnection();
                var cmd = con.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"SELECT id, id_certificado, nombre_curso, nombre_alumno, RUT,
                    certificate_type, footer_1, footer_2, contenidoHtml, footerHtml,
                    fecha, nombre_referencia_curso, imagen_fondo
                    FROM contenido_certificado WHERE id_certificado = @id_certificado";
                cmd.Parameters.AddWithValue("@id_certificado", certificadoId);
                con.Open();
                using var dr = cmd.ExecuteReader();
                if (!dr.Read()) return null;
                return new ContenidoCertificadoDAL
                {
                    Id = GetGuid(dr, dr.GetOrdinal("id")),
                    IdCertificado = GetGuid(dr, dr.GetOrdinal("id_certificado")),
                    NombreCurso = GetString(dr, dr.GetOrdinal("nombre_curso")),
                    NombreAlumno = GetString(dr, dr.GetOrdinal("nombre_alumno")),
                    RUT = GetString(dr, dr.GetOrdinal("RUT")),
                    CertificateType = GetString(dr, dr.GetOrdinal("certificate_type")),
                    Footer1 = GetString(dr, dr.GetOrdinal("footer_1")),
                    Footer2 = GetString(dr, dr.GetOrdinal("footer_2")),
                    ContenidoHtml = GetString(dr, dr.GetOrdinal("contenidoHtml")),
                    FooterHtml = GetString(dr, dr.GetOrdinal("footerHtml")),
                    Fecha = GetDateTime(dr, dr.GetOrdinal("fecha")),
                    NombreReferenciaCurso = GetString(dr, dr.GetOrdinal("nombre_referencia_curso")),
                    ImagenFondo = GetString(dr, dr.GetOrdinal("imagen_fondo")),
                };
            }
            catch (Exception ex) { throw ex; }
        }
    }
}
