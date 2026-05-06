using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace QRCerts.Api.DAL
{
    public class Plantilla_certificados : DALBase
    {
        public Guid id { get; set; }
        public string nombre { get; set; }
        public string contenido_cursos { get; set; }
        public string contenido_alumnos { get; set; }

        public string path_docx { get; set; }
        public Guid id_otec { get; set; }

        public Plantilla_certificados()
        {
            id = Guid.NewGuid();
            nombre = string.Empty;
            contenido_cursos = string.Empty;
            contenido_alumnos = string.Empty;
            path_docx = string.Empty;
            id_otec = Guid.NewGuid(); ;
        }

        private static List<Plantilla_certificados> mapeo(SqlDataReader dr)
        {
            List<Plantilla_certificados> lst = new List<Plantilla_certificados>();
            Plantilla_certificados obj;
            if (dr.HasRows)
            {
                int id = dr.GetOrdinal("id");
                int nombre = dr.GetOrdinal("nombre");
                int contenido_cursos = dr.GetOrdinal("contenido_cursos");
                int contenido_alumnos = dr.GetOrdinal("contenido_alumnos");

                int path_docx = dr.GetOrdinal("path_docx");
                int id_otec = dr.GetOrdinal("id_otec");
                while (dr.Read())
                {
                    obj = new Plantilla_certificados();
                    if (!dr.IsDBNull(id)) { obj.id = dr.GetGuid(id); }
                    if (!dr.IsDBNull(nombre)) { obj.nombre = dr.GetString(nombre); }
                    if (!dr.IsDBNull(contenido_cursos)) { obj.contenido_cursos = dr.GetString(contenido_cursos); }
                    if (!dr.IsDBNull(contenido_alumnos)) { obj.contenido_alumnos = dr.GetString(contenido_alumnos); }

                    if (!dr.IsDBNull(path_docx)) { obj.path_docx = dr.GetString(path_docx); }
                    if (!dr.IsDBNull(id_otec)) { obj.id_otec = dr.GetGuid(id_otec); }
                    lst.Add(obj);
                }
            }
            return lst;
        }

        public static List<Plantilla_certificados> read(Guid idOtec)
        {
            try
            {
                List<Plantilla_certificados> lst = new List<Plantilla_certificados>();
                using (SqlConnection con = GetConnection())
                {
                    SqlCommand cmd = con.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = 
                        "SELECT *FROM Plantilla_certificados WHERE id_otec=@id_otec";
                    cmd.Parameters.AddWithValue("@id_otec", idOtec);
                    cmd.Connection.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    lst = mapeo(dr);
                    return lst;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static string GetTemplateFileName(Guid idCurso)
        {
            try
            {
                string ret = string.Empty;
                using (SqlConnection con = GetConnection())
                {
                    SqlCommand cmd = con.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText =
                        @"SELECT B.path_docx FROM cursos A
                        INNER JOIN plantilla_certificados B ON A.PlantillaId=B.id
                        WHERE A.Id = @idCurso";
                    cmd.Parameters.AddWithValue("@idCurso", idCurso);
                    cmd.Connection.Open();
                    ret = Convert.ToString(cmd.ExecuteScalar());
                }
                return ret;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static string GetReplacementJson(Guid courseGuid, Guid alumnoGuid)
        {
            try
            {
                string ret = string.Empty;
                using (SqlConnection con = GetConnection())
                {
                    SqlCommand cmd = con.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText =
                        @"SELECT 
	CASE LayoutJson
		WHEN '' THEN 
		LEFT(Observaciones, LEN(Observaciones) - 1) + ', ' +
		'""NOMBRE_CURSO"": ""' + nombre_visualizar_certificado + '""}'
	ELSE
		LEFT(Observaciones, LEN(Observaciones) - 1) + ',' +
		SUBSTRING(LayoutJson, 2, LEN(LayoutJson) - 2) + ',' + 
		'""NOMBRE_CURSO"": ""' + nombre_visualizar_certificado + '""}'
	END
FROM REGISTRO_ALUMNOS A 
	INNER JOIN Cursos B ON A.id_curso=B.Id
	WHERE id_alumno = @alumnoGuid
	AND id_curso=@courseGuid";

                    cmd.Parameters.AddWithValue("@courseGuid", courseGuid);
                    cmd.Parameters.AddWithValue("@alumnoGuid", alumnoGuid);
                    cmd.Connection.Open();

                    ret = Convert.ToString(cmd.ExecuteScalar());
                }
                return ret;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static Plantilla_certificados getByPk(
        Guid id)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT *FROM Plantilla_certificados WHERE");
                sql.AppendLine("id = @id");
                Plantilla_certificados obj = null;
                using (SqlConnection con = GetConnection())
                {
                    SqlCommand cmd = con.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sql.ToString();
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Connection.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    List<Plantilla_certificados> lst = mapeo(dr);
                    if (lst.Count != 0)
                        obj = lst[0];
                }
                return obj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void insert(Plantilla_certificados obj)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("INSERT INTO Plantilla_certificados(");
                sql.AppendLine("id");
                sql.AppendLine(",nombre");
                sql.AppendLine(", contenido_cursos");
                sql.AppendLine(", contenido_alumnos");
                sql.AppendLine(", path_docx");
                sql.AppendLine(", id_otec");
                sql.AppendLine(")");
                sql.AppendLine("VALUES");
                sql.AppendLine("(");
                sql.AppendLine("@id");
                sql.AppendLine(",@nombre");
                sql.AppendLine(", @contenido_cursos");
                sql.AppendLine(", @contenido_alumnos");
                sql.AppendLine(", @path_docx");
                sql.AppendLine(", @id_otec");
                sql.AppendLine(")");
                using (SqlConnection con = GetConnection())
                {
                    SqlCommand cmd = con.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sql.ToString();
                    cmd.Parameters.AddWithValue("@id", obj.id);
                    cmd.Parameters.AddWithValue("@nombre", obj.nombre);
                    cmd.Parameters.AddWithValue("@contenido_cursos", obj.contenido_cursos);
                    cmd.Parameters.AddWithValue("@contenido_alumnos", obj.contenido_alumnos);
                    cmd.Parameters.AddWithValue("@path_docx", obj.path_docx);
                    cmd.Parameters.AddWithValue("@id_otec", obj.id_otec);
                    cmd.Connection.Open();
                    Convert.ToInt32(cmd.ExecuteNonQuery());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void update(Plantilla_certificados obj)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("UPDATE  Plantilla_certificados SET");
                sql.AppendLine("nombre=@nombre");
                sql.AppendLine(", contenido_cursos=@contenido_cursos");
                sql.AppendLine(", contenido_alumnos=@contenido_alumnos");
                sql.AppendLine(", path_docx=@path_docx");
                sql.AppendLine(", id_otec=@id_otec");
                sql.AppendLine("WHERE");
                sql.AppendLine("id=@id");
                using (SqlConnection con = GetConnection())
                {
                    SqlCommand cmd = con.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sql.ToString();
                    cmd.Parameters.AddWithValue("@nombre", obj.nombre);
                    cmd.Parameters.AddWithValue("@contenido_cursos", obj.contenido_cursos);
                    cmd.Parameters.AddWithValue("@contenido_alumnos", obj.contenido_alumnos); cmd.Parameters.AddWithValue("@path_docx", obj.path_docx);
                    cmd.Parameters.AddWithValue("@id_otec", obj.id_otec);
                    cmd.Parameters.AddWithValue("@id", obj.id);
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void delete(Plantilla_certificados obj)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("DELETE  Plantilla_certificados ");
                sql.AppendLine("WHERE");
                sql.AppendLine("id=@id");
                using (SqlConnection con = GetConnection())
                {
                    SqlCommand cmd = con.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sql.ToString();
                    cmd.Parameters.AddWithValue("@id", obj.id);
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}

