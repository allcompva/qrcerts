// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.DAL.AlumnosDAL
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using Microsoft.Data.SqlClient;
using QRCerts.Api.Models;
using System;
using System.Collections.Generic;
using System.Data;

#nullable enable
namespace QRCerts.Api.DAL
{
    public class AlumnosDAL : DALBase
    {
        public Guid Id { get; set; }

        public Guid OtecId { get; set; }

        public string NombreApellido { get; set; } = string.Empty;

        public string RUT { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool certificado { get; set; }
        public string observaciones { get; set; }

        public AlumnosDAL()
        {
            this.NombreApellido = string.Empty;
            this.RUT = string.Empty;
            this.certificado = false;
        }

        private static List<Alumno> mapeo(SqlDataReader dr)
        {
            List<Alumno> alumnoList = new List<Alumno>();
            if (dr.HasRows)
            {
                int ordinal1 = dr.GetOrdinal("Id");
                int ordinal2 = dr.GetOrdinal("OtecId");
                int ordinal3 = dr.GetOrdinal("NombreApellido");
                int ordinal4 = dr.GetOrdinal("RUT");
                int ordinal5 = dr.GetOrdinal("CreatedAt");
                while (dr.Read())
                {
                    Alumno alumno = new Alumno();
                    if (!dr.IsDBNull(ordinal1))
                        alumno.Id = dr.GetGuid(ordinal1);
                    if (!dr.IsDBNull(ordinal2))
                        alumno.OtecId = dr.GetGuid(ordinal2);
                    if (!dr.IsDBNull(ordinal3))
                        alumno.NombreApellido = dr.GetString(ordinal3);
                    if (!dr.IsDBNull(ordinal4))
                        alumno.RUT = dr.GetString(ordinal4);
                    if (!dr.IsDBNull(ordinal5))
                        alumno.CreatedAt = dr.GetDateTime(ordinal5);

                    alumnoList.Add(alumno);
                }
            }
            return alumnoList;
        }

        private static List<Alumno> mapeoWithOtec(SqlDataReader dr)
        {
            List<Alumno> alumnoList = new List<Alumno>();
            if (dr.HasRows)
            {
                int ordinal1 = dr.GetOrdinal("Id");
                int ordinal2 = dr.GetOrdinal("OtecId");
                int ordinal3 = dr.GetOrdinal("NombreApellido");
                int ordinal4 = dr.GetOrdinal("RUT");
                int ordinal5 = dr.GetOrdinal("CreatedAt");

                while (dr.Read())
                {
                    Alumno alumno = new Alumno();
                    if (!dr.IsDBNull(ordinal1))
                        alumno.Id = dr.GetGuid(ordinal1);
                    if (!dr.IsDBNull(ordinal2))
                        alumno.OtecId = dr.GetGuid(ordinal2);
                    if (!dr.IsDBNull(ordinal3))
                        alumno.NombreApellido = dr.GetString(ordinal3);
                    if (!dr.IsDBNull(ordinal4))
                        alumno.RUT = dr.GetString(ordinal4);
                    if (!dr.IsDBNull(ordinal5))
                        alumno.CreatedAt = dr.GetDateTime(ordinal5);

                    alumnoList.Add(alumno);
                }
            }
            return alumnoList;
        }

        private static List<Alumno> mapeoWithRegistro(SqlDataReader dr)
        {
            List<Alumno> alumnoList = new List<Alumno>();
            if (dr.HasRows)
            {
                int Id = dr.GetOrdinal("Id");
                int OtecId = dr.GetOrdinal("OtecId");
                int NombreApellido = dr.GetOrdinal("NombreApellido");
                int RUT = dr.GetOrdinal("RUT");
                int CreatedAt = dr.GetOrdinal("CreatedAt");
                int certificado = dr.GetOrdinal("certificado");
                int observaciones = dr.GetOrdinal("observaciones");

                while (dr.Read())
                {
                    Alumno alumno = new Alumno();
                    if (!dr.IsDBNull(Id))
                        alumno.Id = dr.GetGuid(Id);
                    if (!dr.IsDBNull(OtecId))
                        alumno.OtecId = dr.GetGuid(OtecId);
                    if (!dr.IsDBNull(NombreApellido))
                        alumno.NombreApellido = dr.GetString(NombreApellido);
                    if (!dr.IsDBNull(RUT))
                        alumno.RUT = dr.GetString(RUT);
                    if (!dr.IsDBNull(CreatedAt))
                        alumno.CreatedAt = dr.GetDateTime(CreatedAt);
                    if (!dr.IsDBNull(certificado))
                        alumno.certificado = dr.GetInt32(certificado) == 1;
                    if (!dr.IsDBNull(observaciones))
                        alumno.observaciones = dr.GetString(observaciones);
                    alumnoList.Add(alumno);
                }
            }
            return alumnoList;
        }

        public static List<Alumno> GetByOtecId(Guid otecId)
        {
            try
            {
                List<Alumno> alumnoList = new List<Alumno>();
                using (SqlConnection connection = DALBase.GetConnection())
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT a.Id, a.OtecId, a.NombreApellido, a.RUT, a.CreatedAt,\r\n                                              o.Id, o.Nombre, o.Slug, o.Estado, o.CreatedAt\r\n                                       FROM Alumnos a\r\n                                       LEFT JOIN Otecs o ON a.OtecId = o.Id\r\n                                       WHERE a.OtecId = @OtecId\r\n                                       ORDER BY a.NombreApellido";
                    command.Parameters.AddWithValue("@OtecId", (object)otecId);
                    command.Connection.Open();
                    return AlumnosDAL.mapeoWithOtec(command.ExecuteReader());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Alumno GetById(Guid id)
        {
            try
            {
                string str =
                            @"SELECT * FROM Alumnos a
                    WHERE a.Id = @Id";
                Alumno byId = (Alumno)null;
                using (SqlConnection connection = DALBase.GetConnection())
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = str.ToString();
                    command.Parameters.AddWithValue("@Id", (object)id);
                    command.Connection.Open();
                    List<Alumno> alumnoList = AlumnosDAL.mapeo(command.ExecuteReader());
                    if (alumnoList.Count != 0)
                        byId = alumnoList[0];
                }
                return byId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static bool VerificaDuplicado(string rut, string idCurso)
        {
            try
            {
                bool ret = false;
                string str =
                    @"SELECT B.RUT FROM REGISTRO_ALUMNOS A
                    INNER JOIN Alumnos B ON A.id_alumno=B.Id
                    AND A.id_curso=@id_curso
                    WHERE B.RUT=@rut";
                using (SqlConnection connection = DALBase.GetConnection())
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = str.ToString();
                    command.Parameters.AddWithValue("@id_curso", idCurso);
                    command.Parameters.AddWithValue("@rut", rut);
                    command.Connection.Open();
                    SqlDataReader dr = command.ExecuteReader();
                    if (dr.HasRows)
                        ret = true;
                }
                return ret;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Alumno? GetByOtecIdAndRut(Guid otecId, string rut)
        {
            try
            {
                string str =
                    @"SELECT Id, OtecId, NombreApellido, RUT, CreatedAt
                      FROM Alumnos
                      WHERE OtecId = @OtecId AND RUT = @RUT";
                using (SqlConnection connection = DALBase.GetConnection())
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = str;
                    command.Parameters.AddWithValue("@OtecId", (object)otecId);
                    command.Parameters.AddWithValue("@RUT", (object)rut);
                    command.Connection.Open();
                    List<Alumno> list = mapeo(command.ExecuteReader());
                    return list.Count > 0 ? list[0] : null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Alumno GetByIdCert(Guid idAlumno, Guid idCurso)
        {
            try
            {
                string str =
                            @"SELECT 
                        a.Id, 
                        a.OtecId, 
                        a.NombreApellido, 
                        a.RUT,
                        a.CreatedAt,
                        ISNULL((SELECT 1 FROM Certificados B 
                            WHERE A.Id=B.AlumnoId AND B.CursoId=@id_curso), '') 
                        AS certificado,
                        r.observaciones
                    FROM Alumnos a
                        INNER JOIN REGISTRO_ALUMNOS r ON a.Id = r.id_alumno
                    WHERE a.Id = @Id";

                Alumno byIdCert = (Alumno)null;
                using (SqlConnection connection = DALBase.GetConnection())
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = str.ToString();
                    command.Parameters.AddWithValue("@Id", (object)idAlumno);
                    command.Parameters.AddWithValue("@id_curso", (object)idCurso);
                    command.Connection.Open();
                    List<Alumno> alumnoList = AlumnosDAL.mapeoWithRegistro(command.ExecuteReader());
                    if (alumnoList.Count != 0)
                        byIdCert = alumnoList[0];
                }
                return byIdCert;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Alumno GetByOtecAndRut(Guid otecId, string rut)
        {
            try
            {
                string str =
                    @"SELECT 
                a.Id, 
                a.OtecId, 
                a.NombreApellido, 
                a.RUT, 
                a.CreatedAt,
                o.Id, 
                o.Nombre, 
                o.Slug, 
                o.Estado, 
                o.CreatedAt
            FROM Alumnos a
            LEFT JOIN Otecs o ON a.OtecId = o.Id
            WHERE a.OtecId = @OtecId AND a.RUT = @RUT";
                Alumno byOtecAndRut = (Alumno)null;
                using (SqlConnection connection = DALBase.GetConnection())
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = str.ToString();
                    command.Parameters.AddWithValue("@OtecId", (object)otecId);
                    command.Parameters.AddWithValue("@RUT", (object)rut);
                    command.Connection.Open();
                    List<Alumno> alumnoList = AlumnosDAL.mapeoWithOtec(command.ExecuteReader());
                    if (alumnoList.Count != 0)
                        byOtecAndRut = alumnoList[0];
                }
                return byOtecAndRut;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Guid Insert(Alumno obj)
        {
            try
            {
                obj.Id = Guid.NewGuid();
                obj.CreatedAt = DateTime.UtcNow;
                string str =
                            @"INSERT INTO Alumnos
                    (Id, OtecId, NombreApellido, RUT, CreatedAt)
                    VALUES
                    (@Id, @OtecId, @NombreApellido, @RUT, @CreatedAt)";

                using (SqlConnection connection = DALBase.GetConnection())
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = str.ToString();
                    command.Parameters.AddWithValue("@Id", (object)obj.Id);
                    command.Parameters.AddWithValue("@OtecId", (object)obj.OtecId);
                    command.Parameters.AddWithValue("@NombreApellido", (object)obj.NombreApellido);
                    command.Parameters.AddWithValue("@RUT", (object)obj.RUT);
                    command.Parameters.AddWithValue("@CreatedAt", (object)obj.CreatedAt);

                    command.Connection.Open();
                    command.ExecuteNonQuery();
                    return obj.Id;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Update(Alumno obj)
        {
            try
            {
                string str = "UPDATE Alumnos SET\r\n                    OtecId=@OtecId,\r\n                    NombreApellido=@NombreApellido,\r\n                    RUT=@RUT\r\n                WHERE\r\n                    Id=@Id";
                using (SqlConnection connection = DALBase.GetConnection())
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = str.ToString();
                    command.Parameters.AddWithValue("@Id", (object)obj.Id);
                    command.Parameters.AddWithValue("@OtecId", (object)obj.OtecId);
                    command.Parameters.AddWithValue("@NombreApellido", (object)obj.NombreApellido);
                    command.Parameters.AddWithValue("@RUT", (object)obj.RUT);
                    command.Connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<Alumno> GetByCursoId(Guid cursoId)
        {
            try
            {
                List<Alumno> alumnoList = new List<Alumno>();
                using (SqlConnection connection = DALBase.GetConnection())
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = 
                        @"SELECT DISTINCT
                        a.Id, a.OtecId,
                        a.NombreApellido,
                        a.RUT,
                        a.CreatedAt,
                        r.calificacion,
                        r.observaciones,
                        r.certificado_otorgado,
                        r.motivo_entrega,
                            ISNULL((SELECT 1 FROM Certificados B
                            WHERE A.Id=B.AlumnoId AND B.CursoId=@id_curso),
                            '') AS certificado
                        FROM Alumnos a
                            INNER JOIN REGISTRO_ALUMNOS r 
                            ON a.Id = r.id_alumno
                        WHERE r.id_curso = @id_curso
                        ORDER BY a.NombreApellido";

                    command.Parameters.AddWithValue("@id_curso", (object)cursoId);
                    command.Connection.Open();
                    return AlumnosDAL.mapeoWithRegistro(command.ExecuteReader());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Alumno obj)
        {
            try
            {
                string str = "DELETE FROM Alumnos WHERE Id=@Id";
                using (SqlConnection connection = DALBase.GetConnection())
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = str.ToString();
                    command.Parameters.AddWithValue("@Id", (object)obj.Id);
                    command.Connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
