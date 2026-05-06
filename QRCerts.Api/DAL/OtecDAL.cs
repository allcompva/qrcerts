// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.DAL.OtecDAL
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
  public class OtecDAL : DALBase
  {
    private static List<Otec> mapeo(SqlDataReader dr)
    {
      List<Otec> otecList = new List<Otec>();
      if (dr.HasRows)
      {
        while (dr.Read())
        {
          Otec otec = new Otec();
          if (!dr.IsDBNull(0))
            otec.Id = dr.GetGuid(0);
          if (!dr.IsDBNull(1))
            otec.Nombre = dr.GetString(1);
          if (!dr.IsDBNull(2))
            otec.Slug = dr.GetString(2);
          if (!dr.IsDBNull(3))
            otec.Estado = dr.GetByte(3);
          if (!dr.IsDBNull(4))
            otec.CreatedAt = dr.GetDateTime(4);
          if (!dr.IsDBNull(5))
            otec.MoodleHabilitado = dr.GetBoolean(5);
          otecList.Add(otec);
        }
      }
      return otecList;
    }

    public static List<Otec> GetAll()
    {
      try
      {
        List<Otec> otecList = new List<Otec>();
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = "SELECT Id, Nombre, Slug, Estado, CreatedAt, MoodleHabilitado FROM Otecs ORDER BY Nombre";
          command.Connection.Open();
          return OtecDAL.mapeo(command.ExecuteReader());
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static List<Otec> GetActive()
    {
      try
      {
        List<Otec> otecList = new List<Otec>();
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = "SELECT Id, Nombre, Slug, Estado, CreatedAt, MoodleHabilitado FROM Otecs WHERE Estado = 1 ORDER BY Nombre";
          command.Connection.Open();
          return OtecDAL.mapeo(command.ExecuteReader());
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static Otec GetById(Guid id)
    {
      try
      {
        string str = "SELECT Id, Nombre, Slug, Estado, CreatedAt, MoodleHabilitado FROM Otecs WHERE Id = @Id";
        Otec byId = (Otec) null;
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = str.ToString();
          command.Parameters.AddWithValue("@Id", (object) id);
          command.Connection.Open();
          List<Otec> otecList = OtecDAL.mapeo(command.ExecuteReader());
          if (otecList.Count != 0)
            byId = otecList[0];
        }
        return byId;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static Otec GetBySlug(string slug)
    {
      try
      {
        string str = "SELECT Id, Nombre, Slug, Estado, CreatedAt, MoodleHabilitado FROM Otecs WHERE Slug = @Slug";
        Otec bySlug = (Otec) null;
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = str.ToString();
          command.Parameters.AddWithValue("@Slug", (object) slug);
          command.Connection.Open();
          List<Otec> otecList = OtecDAL.mapeo(command.ExecuteReader());
          if (otecList.Count != 0)
            bySlug = otecList[0];
        }
        return bySlug;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static bool SlugExists(string slug, Guid? excludeId = null)
    {
      try
      {
        string str = "SELECT COUNT(*) FROM Otecs WHERE Slug = @Slug";
        if (excludeId.HasValue)
          str += " AND Id != @ExcludeId";
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = str;
          command.Parameters.AddWithValue("@Slug", (object) slug);
          if (excludeId.HasValue)
            command.Parameters.AddWithValue("@ExcludeId", (object) excludeId.Value);
          command.Connection.Open();
          return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static int GetUserCount(Guid otecId)
    {
      try
      {
        string str = "SELECT COUNT(*) FROM OtecUsers WHERE OtecId = @OtecId";
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = str;
          command.Parameters.AddWithValue("@OtecId", (object) otecId);
          command.Connection.Open();
          return Convert.ToInt32(command.ExecuteScalar());
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static Guid Insert(Otec obj)
    {
      try
      {
        obj.Id = Guid.NewGuid();
        obj.CreatedAt = DateTime.UtcNow;
        obj.Estado = (byte) 1;
        string str = "INSERT INTO Otecs(Id, Nombre, Slug, Estado, CreatedAt, MoodleHabilitado) VALUES (@Id, @Nombre, @Slug, @Estado, @CreatedAt, @MoodleHabilitado)";
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = str.ToString();
          command.Parameters.AddWithValue("@Id", (object) obj.Id);
          command.Parameters.AddWithValue("@Nombre", (object) obj.Nombre);
          command.Parameters.AddWithValue("@Slug", (object) obj.Slug);
          command.Parameters.AddWithValue("@Estado", (object) obj.Estado);
          command.Parameters.AddWithValue("@CreatedAt", (object) obj.CreatedAt);
          command.Parameters.AddWithValue("@MoodleHabilitado", (object) obj.MoodleHabilitado);
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

    public static void Update(Otec obj)
    {
      try
      {
        string str = "UPDATE Otecs SET Nombre=@Nombre, Slug=@Slug, Estado=@Estado, MoodleHabilitado=@MoodleHabilitado WHERE Id=@Id";
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = str.ToString();
          command.Parameters.AddWithValue("@Id", (object) obj.Id);
          command.Parameters.AddWithValue("@Nombre", (object) obj.Nombre);
          command.Parameters.AddWithValue("@Slug", (object) obj.Slug);
          command.Parameters.AddWithValue("@Estado", (object) obj.Estado);
          command.Parameters.AddWithValue("@MoodleHabilitado", (object) obj.MoodleHabilitado);
          command.Connection.Open();
          command.ExecuteNonQuery();
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static void Delete(Otec obj)
    {
      try
      {
        string str = "DELETE FROM Otecs WHERE Id=@Id";
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = str.ToString();
          command.Parameters.AddWithValue("@Id", (object) obj.Id);
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
