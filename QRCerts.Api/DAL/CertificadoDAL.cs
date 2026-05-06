// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.DAL.CertificadoDAL
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using Microsoft.Data.SqlClient;
using QRCerts.Api.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

#nullable enable
namespace QRCerts.Api.DAL
{
  public class CertificadoDAL : DALBase
  {
    private static List<Certificado> mapeo(SqlDataReader dr)
    {
      List<Certificado> certificadoList = new List<Certificado>();
      if (dr.HasRows)
      {
        int ordinal1 = dr.GetOrdinal("Id");
        int ordinal2 = dr.GetOrdinal("CursoId");
        int ordinal3 = dr.GetOrdinal("AlumnoId");
        int ordinal4 = dr.GetOrdinal("Code");
        int ordinal5 = dr.GetOrdinal("PdfFilename");
        int ordinal6 = dr.GetOrdinal("IssuedAt");
        int ordinal7 = dr.GetOrdinal("Estado");
        int ordinal8 = dr.GetOrdinal("url_landing");
        while (dr.Read())
        {
          Certificado certificado = new Certificado();
          if (!dr.IsDBNull(ordinal1))
            certificado.Id = dr.GetGuid(ordinal1);
          if (!dr.IsDBNull(ordinal2))
            certificado.CursoId = dr.GetGuid(ordinal2);
          if (!dr.IsDBNull(ordinal3))
            certificado.AlumnoId = dr.GetGuid(ordinal3);
          if (!dr.IsDBNull(ordinal4))
            certificado.Code = dr.GetString(ordinal4);
          if (!dr.IsDBNull(ordinal5))
            certificado.PdfFilename = dr.GetString(ordinal5);
          if (!dr.IsDBNull(ordinal6))
            certificado.IssuedAt = dr.GetDateTime(ordinal6);
          if (!dr.IsDBNull(ordinal7))
            certificado.Estado = dr.GetByte(ordinal7);
          if (!dr.IsDBNull(ordinal8))
            certificado.url_landing = dr.GetString(ordinal8);
          certificadoList.Add(certificado);
        }
      }
      return certificadoList;
    }

    public static List<Certificado> read()
    {
      try
      {
        List<Certificado> certificadoList = new List<Certificado>();
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = "SELECT *FROM Certificados";
          command.Connection.Open();
          return CertificadoDAL.mapeo(command.ExecuteReader());
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static Certificado getByPk(int Id)
    {
      try
      {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("SELECT *FROM Certificados WHERE");
        stringBuilder.AppendLine("Id = @Id");
        Certificado byPk = (Certificado) null;
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = stringBuilder.ToString();
          command.Parameters.AddWithValue("@Id", (object) Id);
          command.Connection.Open();
          List<Certificado> certificadoList = CertificadoDAL.mapeo(command.ExecuteReader());
          if (certificadoList.Count != 0)
            byPk = certificadoList[0];
        }
        return byPk;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static Certificado getByGuid(Guid AlumnoId, Guid CursoId)
    {
      try
      {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("SELECT *FROM Certificados WHERE");
        stringBuilder.AppendLine("AlumnoId = @AlumnoId AND CursoId=@CursoId");
        Certificado byGuid = (Certificado) null;
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = stringBuilder.ToString();
          command.Parameters.AddWithValue("@AlumnoId", (object) AlumnoId);
          command.Parameters.AddWithValue("@CursoId", (object) CursoId);
          command.Connection.Open();
          List<Certificado> certificadoList = CertificadoDAL.mapeo(command.ExecuteReader());
          if (certificadoList.Count != 0)
            byGuid = certificadoList[0];
        }
        return byGuid;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static int insert(Certificado obj)
    {
      try
      {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("INSERT INTO Certificados(");
        stringBuilder.AppendLine("Id");
        stringBuilder.AppendLine(", CursoId");
        stringBuilder.AppendLine(", AlumnoId");
        stringBuilder.AppendLine(", Code");
        stringBuilder.AppendLine(", PdfFilename");
        stringBuilder.AppendLine(", IssuedAt");
        stringBuilder.AppendLine(", Estado");
        stringBuilder.AppendLine(", url_landing");
        stringBuilder.AppendLine(")");
        stringBuilder.AppendLine("VALUES");
        stringBuilder.AppendLine("(");
        stringBuilder.AppendLine("@Id");
        stringBuilder.AppendLine(", @CursoId");
        stringBuilder.AppendLine(", @AlumnoId");
        stringBuilder.AppendLine(", @Code");
        stringBuilder.AppendLine(", @PdfFilename");
        stringBuilder.AppendLine(", @IssuedAt");
        stringBuilder.AppendLine(", @Estado");
        stringBuilder.AppendLine(", @url_landing");
        stringBuilder.AppendLine(")");
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = stringBuilder.ToString();
          command.Parameters.AddWithValue("@Id", (object) obj.Id);
          command.Parameters.AddWithValue("@CursoId", (object) obj.CursoId);
          command.Parameters.AddWithValue("@AlumnoId", (object) obj.AlumnoId);
          command.Parameters.AddWithValue("@Code", (object) obj.Code);
          command.Parameters.AddWithValue("@PdfFilename", (object) obj.PdfFilename);
          command.Parameters.AddWithValue("@IssuedAt", (object) obj.IssuedAt);
          command.Parameters.AddWithValue("@Estado", (object) obj.Estado);
          command.Parameters.AddWithValue("@url_landing", (object) obj.url_landing);
          command.Connection.Open();
          return command.ExecuteNonQuery();
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static int update(Certificado obj)
    {
      try
      {
        obj.IssuedAt = DateTime.Now;
        obj.Estado = (byte) 0;
        obj.Id = Guid.NewGuid();
        string str = @"UPDATE Certificados
                    SET Code=@Code, PdfFilename=@PdfFilename,
                    IssuedAt=@IssuedAt, Estado=@Estado, url_landing=@url_landing
                    WHERE CursoId=@CursoId AND AlumnoId=@AlumnoId";
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = str.ToString();
          command.Parameters.AddWithValue("@CursoId", (object) obj.CursoId);
          command.Parameters.AddWithValue("@AlumnoId", (object) obj.AlumnoId);
          command.Parameters.AddWithValue("@Code", (object) obj.Code);
          command.Parameters.AddWithValue("@PdfFilename", (object) obj.PdfFilename);
          command.Parameters.AddWithValue("@IssuedAt", (object) obj.IssuedAt);
          command.Parameters.AddWithValue("@Estado", (object) obj.Estado);
          command.Parameters.AddWithValue("@url_landing", (object) obj.url_landing);
          command.Connection.Open();
          return command.ExecuteNonQuery();
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static int delete(Guid idAlumno, Guid idCurso)
    {
      try
      {
        string str = "DELETE Certificados\r\n                    WHERE CursoId=@CursoId AND AlumnoId=@AlumnoId";
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = str.ToString();
          command.Parameters.AddWithValue("@CursoId", (object) idCurso);
          command.Parameters.AddWithValue("@AlumnoId", (object) idAlumno);
          command.Connection.Open();
          return command.ExecuteNonQuery();
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }
  }
}
