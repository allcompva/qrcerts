// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.DAL.OtecUserDAL
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
  public class OtecUserDAL : DALBase
  {
    private static List<OtecUser> mapeo(SqlDataReader dr)
    {
      List<OtecUser> otecUserList = new List<OtecUser>();
      if (dr.HasRows)
      {
        while (dr.Read())
        {
          OtecUser otecUser = new OtecUser();
          if (!dr.IsDBNull(0))
            otecUser.Id = dr.GetGuid(0);
          if (!dr.IsDBNull(1))
            otecUser.OtecId = dr.GetGuid(1);
          if (!dr.IsDBNull(2))
            otecUser.NombreApellido = dr.GetString(2);
          if (!dr.IsDBNull(3))
            otecUser.RUT = dr.GetString(3);
          if (!dr.IsDBNull(4))
            otecUser.Email = dr.GetString(4);
          if (!dr.IsDBNull(5))
            otecUser.Username = dr.GetString(5);
          if (!dr.IsDBNull(6))
            otecUser.PasswordHash = dr.GetString(6);
          if (!dr.IsDBNull(7))
            otecUser.Estado = dr.GetByte(7);
          if (!dr.IsDBNull(8))
            otecUser.CreatedAt = dr.GetDateTime(8);
          if (!dr.IsDBNull(9))
            otecUser.LastLoginAt = new DateTime?(dr.GetDateTime(9));
          otecUserList.Add(otecUser);
        }
      }
      return otecUserList;
    }

    private static List<OtecUser> mapeoWithOtec(SqlDataReader dr)
    {
      List<OtecUser> otecUserList = new List<OtecUser>();
      if (dr.HasRows)
      {
        while (dr.Read())
        {
          OtecUser otecUser = new OtecUser();
          if (!dr.IsDBNull(0))
            otecUser.Id = dr.GetGuid(0);
          if (!dr.IsDBNull(1))
            otecUser.OtecId = dr.GetGuid(1);
          if (!dr.IsDBNull(2))
            otecUser.NombreApellido = dr.GetString(2);
          if (!dr.IsDBNull(3))
            otecUser.RUT = dr.GetString(3);
          if (!dr.IsDBNull(4))
            otecUser.Email = dr.GetString(4);
          if (!dr.IsDBNull(5))
            otecUser.Username = dr.GetString(5);
          if (!dr.IsDBNull(6))
            otecUser.PasswordHash = dr.GetString(6);
          if (!dr.IsDBNull(7))
            otecUser.Estado = dr.GetByte(7);
          if (!dr.IsDBNull(8))
            otecUser.CreatedAt = dr.GetDateTime(8);
          if (!dr.IsDBNull(9))
            otecUser.LastLoginAt = new DateTime?(dr.GetDateTime(9));
          if (!dr.IsDBNull(10))
            otecUser.Otec = new Otec()
            {
              Id = dr.GetGuid(10),
              Nombre = dr.IsDBNull(11) ? string.Empty : dr.GetString(11),
              Slug = dr.IsDBNull(12) ? string.Empty : dr.GetString(12),
              Estado = dr.IsDBNull(13) ? (byte) 0 : dr.GetByte(13),
              CreatedAt = dr.IsDBNull(14) ? DateTime.MinValue : dr.GetDateTime(14),
              MoodleHabilitado = dr.IsDBNull(15) ? false : dr.GetBoolean(15)
            };
          otecUserList.Add(otecUser);
        }
      }
      return otecUserList;
    }

    public static List<OtecUser> GetAll()
    {
      try
      {
        List<OtecUser> otecUserList = new List<OtecUser>();
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = "SELECT u.Id, u.OtecId, u.NombreApellido, u.RUT, u.Email, u.Username, u.PasswordHash, u.Estado, u.CreatedAt, u.LastLoginAt, o.Id, o.Nombre, o.Slug, o.Estado, o.CreatedAt, o.MoodleHabilitado FROM OtecUsers u LEFT JOIN Otecs o ON u.OtecId = o.Id ORDER BY u.NombreApellido";
          command.Connection.Open();
          return OtecUserDAL.mapeoWithOtec(command.ExecuteReader());
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static List<OtecUser> GetByOtecId(Guid otecId)
    {
      try
      {
        List<OtecUser> otecUserList = new List<OtecUser>();
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = "SELECT u.Id, u.OtecId, u.NombreApellido, u.RUT, u.Email, u.Username, u.PasswordHash, u.Estado, u.CreatedAt, u.LastLoginAt, o.Id, o.Nombre, o.Slug, o.Estado, o.CreatedAt, o.MoodleHabilitado FROM OtecUsers u LEFT JOIN Otecs o ON u.OtecId = o.Id WHERE u.OtecId = @OtecId ORDER BY u.NombreApellido";
          command.Parameters.AddWithValue("@OtecId", (object) otecId);
          command.Connection.Open();
          return OtecUserDAL.mapeoWithOtec(command.ExecuteReader());
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static OtecUser GetById(Guid id)
    {
      try
      {
        string str = "SELECT u.Id, u.OtecId, u.NombreApellido, u.RUT, u.Email, u.Username, u.PasswordHash, u.Estado, u.CreatedAt, u.LastLoginAt, o.Id, o.Nombre, o.Slug, o.Estado, o.CreatedAt, o.MoodleHabilitado FROM OtecUsers u LEFT JOIN Otecs o ON u.OtecId = o.Id WHERE u.Id = @Id";
        OtecUser byId = (OtecUser) null;
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = str.ToString();
          command.Parameters.AddWithValue("@Id", (object) id);
          command.Connection.Open();
          List<OtecUser> otecUserList = OtecUserDAL.mapeoWithOtec(command.ExecuteReader());
          if (otecUserList.Count != 0)
            byId = otecUserList[0];
        }
        return byId;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static OtecUser GetByUsername(string username)
    {
      try
      {
        string str = "SELECT u.Id, u.OtecId, u.NombreApellido, u.RUT, u.Email, u.Username, u.PasswordHash, u.Estado, u.CreatedAt, u.LastLoginAt, o.Id, o.Nombre, o.Slug, o.Estado, o.CreatedAt, o.MoodleHabilitado FROM OtecUsers u LEFT JOIN Otecs o ON u.OtecId = o.Id WHERE u.Username = @Username AND u.Estado = 1";
        OtecUser byUsername = (OtecUser) null;
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = str.ToString();
          command.Parameters.AddWithValue("@Username", (object) username);
          command.Connection.Open();
          List<OtecUser> otecUserList = OtecUserDAL.mapeoWithOtec(command.ExecuteReader());
          if (otecUserList.Count != 0)
            byUsername = otecUserList[0];
        }
        return byUsername;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static bool UsernameExists(string username, Guid? excludeId = null)
    {
      try
      {
        string str = "SELECT COUNT(*) FROM OtecUsers WHERE Username = @Username";
        if (excludeId.HasValue)
          str += " AND Id != @ExcludeId";
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = str;
          command.Parameters.AddWithValue("@Username", (object) username);
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

    public static Guid Insert(OtecUser obj)
    {
      try
      {
        obj.Id = Guid.NewGuid();
        obj.CreatedAt = DateTime.UtcNow;
        obj.Estado = (byte) 1;
        string str = "INSERT INTO OtecUsers(\n                    Id, OtecId, NombreApellido, RUT, Email, Username, PasswordHash, Estado, CreatedAt, LastLoginAt\n                )\n                VALUES\n                (\n                    @Id, @OtecId, @NombreApellido, @RUT, @Email, @Username, @PasswordHash, @Estado, @CreatedAt, @LastLoginAt\n                )";
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = str.ToString();
          command.Parameters.AddWithValue("@Id", (object) obj.Id);
          command.Parameters.AddWithValue("@OtecId", (object) obj.OtecId);
          command.Parameters.AddWithValue("@NombreApellido", (object) obj.NombreApellido);
          command.Parameters.AddWithValue("@RUT", (object) obj.RUT);
          command.Parameters.AddWithValue("@Email", (object) obj.Email);
          command.Parameters.AddWithValue("@Username", (object) obj.Username);
          command.Parameters.AddWithValue("@PasswordHash", (object) obj.PasswordHash);
          command.Parameters.AddWithValue("@Estado", (object) obj.Estado);
          command.Parameters.AddWithValue("@CreatedAt", (object) obj.CreatedAt);
          command.Parameters.AddWithValue("@LastLoginAt", (object) obj.LastLoginAt ?? (object) DBNull.Value);
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

    public static void Update(OtecUser obj)
    {
      try
      {
        string str = "UPDATE OtecUsers SET\n                    OtecId=@OtecId,\n                    NombreApellido=@NombreApellido,\n                    RUT=@RUT,\n                    Email=@Email,\n                    Username=@Username,\n                    PasswordHash=@PasswordHash,\n                    Estado=@Estado,\n                    LastLoginAt=@LastLoginAt\n                WHERE\n                    Id=@Id";
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = str.ToString();
          command.Parameters.AddWithValue("@Id", (object) obj.Id);
          command.Parameters.AddWithValue("@OtecId", (object) obj.OtecId);
          command.Parameters.AddWithValue("@NombreApellido", (object) obj.NombreApellido);
          command.Parameters.AddWithValue("@RUT", (object) obj.RUT);
          command.Parameters.AddWithValue("@Email", (object) obj.Email);
          command.Parameters.AddWithValue("@Username", (object) obj.Username);
          command.Parameters.AddWithValue("@PasswordHash", (object) obj.PasswordHash);
          command.Parameters.AddWithValue("@Estado", (object) obj.Estado);
          command.Parameters.AddWithValue("@LastLoginAt", (object) obj.LastLoginAt ?? (object) DBNull.Value);
          command.Connection.Open();
          command.ExecuteNonQuery();
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static void UpdateLastLogin(Guid id)
    {
      try
      {
        string str = "UPDATE OtecUsers SET LastLoginAt=@LastLoginAt WHERE Id=@Id";
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = str.ToString();
          command.Parameters.AddWithValue("@Id", (object) id);
          command.Parameters.AddWithValue("@LastLoginAt", (object) DateTime.UtcNow);
          command.Connection.Open();
          command.ExecuteNonQuery();
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static void Delete(OtecUser obj)
    {
      try
      {
        string str = "DELETE FROM OtecUsers WHERE Id=@Id";
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
