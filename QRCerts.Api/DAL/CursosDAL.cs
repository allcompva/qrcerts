// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.DAL.CursosDAL
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using Microsoft.Data.SqlClient;
using Org.BouncyCastle.Tls;
using QRCerts.Api.Models;
using System;
using System.Collections.Generic;
using System.Data;

#nullable enable
namespace QRCerts.Api.DAL
{
    public class CursosDAL : DALBase
    {
        private static List<Curso> mapeo(SqlDataReader dr)
        {
            List<Curso> cursoList = new List<Curso>();
            if (dr.HasRows)
            {
                int Id = dr.GetOrdinal("Id");
                int OtecId = dr.GetOrdinal("OtecId");
                int NombreReferencia = dr.GetOrdinal("NombreReferencia");
                int QrDestino = dr.GetOrdinal("QrDestino");
                int FondoPath = dr.GetOrdinal("FondoPath");
                int LayoutJson = dr.GetOrdinal("LayoutJson");
                int Estado = dr.GetOrdinal("Estado");
                int IsFondoBloqueado = dr.GetOrdinal("IsFondoBloqueado");
                int IsLayoutBloqueado = dr.GetOrdinal("IsLayoutBloqueado");
                int CreatedAt = dr.GetOrdinal("CreatedAt");
                int UpdatedAt = dr.GetOrdinal("UpdatedAt");
                int footer_1 = dr.GetOrdinal("footer_1");
                int footer_2 = dr.GetOrdinal("footer_2");
                int nombre_visualizar_certificado = dr.GetOrdinal("nombre_visualizar_certificado");
                int certificate_type = dr.GetOrdinal("certificate_type");
                int contenidoHtml = dr.GetOrdinal("contenidoHtml");
                int footerHtml = dr.GetOrdinal("footerHtml");
                int vencimiento = dr.GetOrdinal("vencimiento");
                int PlantillaId = dr.GetOrdinal("PlantillaId");

                while (dr.Read())
                {
                    Curso curso = new Curso();
                    if (!dr.IsDBNull(Id))
                        curso.Id = dr.GetGuid(Id);
                    if (!dr.IsDBNull(OtecId))
                        curso.OtecId = dr.GetGuid(OtecId);
                    if (!dr.IsDBNull(NombreReferencia))
                        curso.NombreReferencia = dr.GetString(NombreReferencia);
                    if (!dr.IsDBNull(QrDestino))
                        curso.QrDestino = dr.GetByte(QrDestino);
                    if (!dr.IsDBNull(FondoPath))
                        curso.FondoPath = dr.GetString(FondoPath);
                    if (!dr.IsDBNull(LayoutJson))
                        curso.LayoutJson = dr.GetString(LayoutJson);
                    if (!dr.IsDBNull(Estado))
                        curso.Estado = dr.GetByte(Estado);
                    if (!dr.IsDBNull(IsFondoBloqueado))
                        curso.IsFondoBloqueado = dr.GetBoolean(IsFondoBloqueado);
                    if (!dr.IsDBNull(IsLayoutBloqueado))
                        curso.IsLayoutBloqueado = dr.GetBoolean(IsLayoutBloqueado);
                    if (!dr.IsDBNull(CreatedAt))
                        curso.CreatedAt = dr.GetDateTime(CreatedAt);
                    if (!dr.IsDBNull(UpdatedAt))
                        curso.UpdatedAt = dr.GetDateTime(UpdatedAt);
                    if (!dr.IsDBNull(footer_1))
                        curso.footer_1 = dr.GetString(footer_1);
                    if (!dr.IsDBNull(footer_2))
                        curso.footer_2 = dr.GetString(footer_2);
                    if (!dr.IsDBNull(nombre_visualizar_certificado))
                        curso.nombre_visualizar_certificado = dr.GetString(nombre_visualizar_certificado);
                    if (!dr.IsDBNull(certificate_type))
                        curso.certificate_type = dr.GetString(certificate_type);
                    if (!dr.IsDBNull(contenidoHtml))
                        curso.contenidoHtml = dr.GetString(contenidoHtml);
                    if (!dr.IsDBNull(footerHtml))
                        curso.footerHtml = dr.GetString(footerHtml);
                    if (!dr.IsDBNull(vencimiento))
                        curso.vencimiento = dr.GetDateTime(vencimiento);
                    if (!dr.IsDBNull(PlantillaId))
                        curso.PlantillaId = dr.GetGuid(PlantillaId);
                    Otec objOtec = OtecDAL.GetById(curso.OtecId);
                    curso.Otec = objOtec;
                    cursoList.Add(curso);
                }
            }
            return cursoList;
        }

        public static List<Curso> GetAll()
        {
            try
            {
                List<Curso> cursoList = new List<Curso>();
                using (SqlConnection connection = DALBase.GetConnection())
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT * FROM Cursos ORDER BY UpdatedAt DESC";
                    command.Connection.Open();
                    return CursosDAL.mapeo(command.ExecuteReader());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static List<Curso> GetByOtec(Guid idOtec)
        {
            try
            {
                List<Curso> cursoList = new List<Curso>();
                using (SqlConnection connection = DALBase.GetConnection())
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText =
                        @"SELECT * FROM Cursos 
                        WHERE OtecId = @OtecId
                        ORDER BY UpdatedAt DESC";

                    command.Parameters.AddWithValue("@OtecId", idOtec);
                    command.Connection.Open();
                    return CursosDAL.mapeo(command.ExecuteReader());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static Curso GetById(Guid id)
        {
            try
            {
                string str = "SELECT * FROM Cursos WHERE Id = @Id";
                Curso byId = (Curso)null;
                using (SqlConnection connection = DALBase.GetConnection())
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = str.ToString();
                    command.Parameters.AddWithValue("@Id", (object)id);
                    command.Connection.Open();
                    List<Curso> cursoList = CursosDAL.mapeo(command.ExecuteReader());
                    if (cursoList.Count != 0)
                        byId = cursoList[0];
                }
                return byId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Guid Insert(Curso obj)
        {
            try
            {
                obj.Id = Guid.NewGuid();
                obj.CreatedAt = DateTime.UtcNow;
                obj.UpdatedAt = DateTime.UtcNow;
                string str =
                    @"INSERT INTO Cursos
                    (Id, 
                     OtecId,    
                     NombreReferencia, 
                     QrDestino, FondoPath, 
                     LayoutJson, 
                     Estado,
                     IsFondoBloqueado, 
                     IsLayoutBloqueado, 
                     CreatedAt, 
                     UpdatedAt, 
                     footer_1, 
                     footer_2, 
                     nombre_visualizar_certificado,
                     certificate_type,
                     contenidoHtml,
                     footerHtml,
                     vencimiento,
                     PlantillaId)
                    VALUES
                    (@Id, 
                     @OtecId, 
                     @NombreReferencia, 
                     @QrDestino, 
                     @FondoPath,
                     @LayoutJson, 
                     @Estado, 
                     @IsFondoBloqueado, 
                     @IsLayoutBloqueado,
                     @CreatedAt, 
                     @UpdatedAt, 
                     @footer_1,
                     @footer_2, 
                     @nombre_visualizar_certificado,
                     @certificate_type,
                     @contenidoHtml,
                     @footerHtml,
                     @vencimiento,
                     @PlantillaId)";

                using (SqlConnection connection = DALBase.GetConnection())
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = str.ToString();
                    command.Parameters.AddWithValue("@Id", (object)obj.Id);
                    command.Parameters.AddWithValue("@OtecId", (object)obj.OtecId);
                    command.Parameters.AddWithValue("@NombreReferencia", (object)obj.NombreReferencia);
                    command.Parameters.AddWithValue("@QrDestino", (object)obj.QrDestino);
                    command.Parameters.AddWithValue("@FondoPath", (object)obj.FondoPath);
                    command.Parameters.AddWithValue("@LayoutJson", (object)obj.LayoutJson ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Estado", (object)obj.Estado);
                    command.Parameters.AddWithValue("@IsFondoBloqueado", (object)obj.IsFondoBloqueado);
                    command.Parameters.AddWithValue("@IsLayoutBloqueado", (object)obj.IsLayoutBloqueado);
                    command.Parameters.AddWithValue("@CreatedAt", (object)obj.CreatedAt);
                    command.Parameters.AddWithValue("@UpdatedAt", (object)obj.UpdatedAt);
                    command.Parameters.AddWithValue("@footer_1", obj.footer_1);
                    command.Parameters.AddWithValue("@footer_2", obj.footer_2);
                    command.Parameters.AddWithValue("@nombre_visualizar_certificado", obj.nombre_visualizar_certificado);
                    command.Parameters.AddWithValue("@certificate_type", obj.certificate_type);
                    command.Parameters.AddWithValue("@contenidoHtml", obj.contenidoHtml);
                    command.Parameters.AddWithValue("@footerHtml", obj.footerHtml);
                    command.Parameters.AddWithValue("@vencimiento", obj.vencimiento);
                    SqlParameter id_plantilla = new SqlParameter();
                    id_plantilla.ParameterName = "@PlantillaId";
                    id_plantilla.SqlDbType = SqlDbType.UniqueIdentifier;
                    id_plantilla.Value = obj.PlantillaId;
                    command.Parameters.Add(id_plantilla);
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

        public static void Update(Curso obj)
        {
            try
            {
                obj.UpdatedAt = DateTime.UtcNow;
                string str =
                    @"UPDATE Cursos SET 
                        OtecId=@OtecId,
                        NombreReferencia=@NombreReferencia,
                        QrDestino=@QrDestino,
                        FondoPath=@FondoPath,
                        LayoutJson=@LayoutJson,
                        Estado=@Estado,
                        IsFondoBloqueado=@IsFondoBloqueado,
                        IsLayoutBloqueado=@IsLayoutBloqueado,
                        UpdatedAt=@UpdatedAt,
                        footer_1=@footer_1,
                        footer_2=@footer_2,
                        nombre_visualizar_certificado=@nombre_visualizar_certificado,
                        certificate_type=@certificate_type,
                        contenidoHtml=@contenidoHtml,
                        footerHtml=@footerHtml,
                        vencimiento=@vencimiento,
                        PlantillaId=@PlantillaId
                    WHERE Id=@Id";
                using (SqlConnection connection = DALBase.GetConnection())
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = str.ToString();
                    command.Parameters.AddWithValue("@Id", (object)obj.Id);
                    command.Parameters.AddWithValue("@OtecId", (object)obj.OtecId);
                    command.Parameters.AddWithValue("@NombreReferencia", (object)obj.NombreReferencia);
                    command.Parameters.AddWithValue("@QrDestino", (object)obj.QrDestino);
                    command.Parameters.AddWithValue("@FondoPath", (object)obj.FondoPath);
                    command.Parameters.AddWithValue("@LayoutJson", (object)obj.LayoutJson ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Estado", (object)obj.Estado);
                    command.Parameters.AddWithValue("@IsFondoBloqueado", (object)obj.IsFondoBloqueado);
                    command.Parameters.AddWithValue("@IsLayoutBloqueado", (object)obj.IsLayoutBloqueado);
                    command.Parameters.AddWithValue("@UpdatedAt", (object)obj.UpdatedAt);
                    command.Parameters.AddWithValue("@footer_1", obj.footer_1);
                    command.Parameters.AddWithValue("@footer_2", obj.footer_2);
                    command.Parameters.AddWithValue("@nombre_visualizar_certificado", obj.nombre_visualizar_certificado);
                    command.Parameters.AddWithValue("@certificate_type", obj.certificate_type);
                    command.Parameters.AddWithValue("@contenidoHtml", obj.contenidoHtml);
                    command.Parameters.AddWithValue("@footerHtml", obj.footerHtml);
                    command.Parameters.AddWithValue("@vencimiento", obj.vencimiento);
                    SqlParameter id_plantilla = new SqlParameter();
                    id_plantilla.ParameterName = "@PlantillaId";
                    id_plantilla.SqlDbType = SqlDbType.UniqueIdentifier;
                    id_plantilla.Value = obj.PlantillaId;
                    command.Parameters.Add(id_plantilla);

                    command.Connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Curso obj)
        {
            try
            {
                string str = "DELETE FROM Cursos WHERE Id=@Id";
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
