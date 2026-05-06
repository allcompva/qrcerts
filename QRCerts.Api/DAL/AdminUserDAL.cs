// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.DAL.AdminUserDAL
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
  public class AdminUserDAL : DALBase
  {
    private static List<AdminUser> mapeo(SqlDataReader dr)
    {
      List<AdminUser> adminUserList = new List<AdminUser>();
      if (dr.HasRows)
      {
        while (dr.Read())
        {
          AdminUser adminUser = new AdminUser();
          if (!dr.IsDBNull(0))
            adminUser.Id = dr.GetString(0);
          if (!dr.IsDBNull(1))
            adminUser.Username = dr.GetString(1);
          if (!dr.IsDBNull(2))
            adminUser.PasswordHash = dr.GetString(2);
          if (!dr.IsDBNull(3))
            adminUser.Email = dr.GetString(3);
          if (!dr.IsDBNull(4))
            adminUser.FullName = dr.GetString(4);
          if (!dr.IsDBNull(5))
            adminUser.IsActive = dr.GetBoolean(5);
          if (!dr.IsDBNull(6))
            adminUser.CreatedAt = dr.GetDateTime(6);
          if (!dr.IsDBNull(7))
            adminUser.LastLoginAt = new DateTime?(dr.GetDateTime(7));
          adminUserList.Add(adminUser);
        }
      }
      return adminUserList;
    }

    public static List<AdminUser> GetAll()
    {
      try
      {
        List<AdminUser> adminUserList = new List<AdminUser>();
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = "SELECT Id, Username, PasswordHash, Email, FullName, IsActive, CreatedAt, LastLoginAt FROM AdminUsers ORDER BY Username";
          command.Connection.Open();
          return AdminUserDAL.mapeo(command.ExecuteReader());
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static AdminUser GetById(string id)
    {
      try
      {
        string str = "SELECT Id, Username, PasswordHash, Email, FullName, IsActive, CreatedAt, LastLoginAt\n                              FROM AdminUsers WHERE Id = @Id";
        AdminUser byId = (AdminUser) null;
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = str.ToString();
          command.Parameters.AddWithValue("@Id", (object) id);
          command.Connection.Open();
          List<AdminUser> adminUserList = AdminUserDAL.mapeo(command.ExecuteReader());
          if (adminUserList.Count != 0)
            byId = adminUserList[0];
        }
        return byId;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static AdminUser GetByUsername(string username)
    {
      try
      {
        string str = "SELECT Id, Username, PasswordHash, Email, FullName, IsActive, CreatedAt, LastLoginAt\n FROM AdminUsers WHERE Username = @Username AND IsActive = 1";
        AdminUser byUsername = (AdminUser) null;
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = str.ToString();
          command.Parameters.AddWithValue("@Username", (object) username);
          command.Connection.Open();
          List<AdminUser> adminUserList = AdminUserDAL.mapeo(command.ExecuteReader());
          if (adminUserList.Count != 0)
            byUsername = adminUserList[0];
        }
        return byUsername;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static bool UsernameExists(string username, string excludeId = null)
    {
      try
      {
        string str = "SELECT COUNT(*) FROM AdminUsers WHERE Username = @Username";
        if (!string.IsNullOrEmpty(excludeId))
          str += " AND Id != @ExcludeId";
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = str;
          command.Parameters.AddWithValue("@Username", (object) username);
          if (!string.IsNullOrEmpty(excludeId))
            command.Parameters.AddWithValue("@ExcludeId", (object) excludeId);
          command.Connection.Open();
          return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static string Insert(AdminUser obj)
    {
      try
      {
        obj.Id = Guid.NewGuid().ToString();
        obj.CreatedAt = DateTime.UtcNow;
        string str = "INSERT INTO AdminUsers(\n                    Id, Username, PasswordHash, Email, FullName, IsActive, CreatedAt, LastLoginAt\n                )\n                VALUES\n                (\n                    @Id, @Username, @PasswordHash, @Email, @FullName, @IsActive, @CreatedAt, @LastLoginAt\n                )";
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = str.ToString();
          command.Parameters.AddWithValue("@Id", (object) obj.Id);
          command.Parameters.AddWithValue("@Username", (object) obj.Username);
          command.Parameters.AddWithValue("@PasswordHash", (object) obj.PasswordHash);
          command.Parameters.AddWithValue("@Email", (object) obj.Email ?? (object) DBNull.Value);
          command.Parameters.AddWithValue("@FullName", (object) obj.FullName ?? (object) DBNull.Value);
          command.Parameters.AddWithValue("@IsActive", (object) obj.IsActive);
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

    public static void Update(AdminUser obj)
    {
      try
      {
        string str = "UPDATE AdminUsers SET\n                    Username=@Username,\n                    PasswordHash=@PasswordHash,\n                    Email=@Email,\n                    FullName=@FullName,\n                    IsActive=@IsActive,\n                    LastLoginAt=@LastLoginAt\n                WHERE\n                    Id=@Id";
        using (SqlConnection connection = DALBase.GetConnection())
        {
          SqlCommand command = connection.CreateCommand();
          command.CommandType = CommandType.Text;
          command.CommandText = str.ToString();
          command.Parameters.AddWithValue("@Id", (object) obj.Id);
          command.Parameters.AddWithValue("@Username", (object) obj.Username);
          command.Parameters.AddWithValue("@PasswordHash", (object) obj.PasswordHash);
          command.Parameters.AddWithValue("@Email", (object) obj.Email ?? (object) DBNull.Value);
          command.Parameters.AddWithValue("@FullName", (object) obj.FullName ?? (object) DBNull.Value);
          command.Parameters.AddWithValue("@IsActive", (object) obj.IsActive);
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

    public static void UpdateLastLogin(string id)
    {
      try
      {
        string str = "UPDATE AdminUsers SET LastLoginAt=@LastLoginAt WHERE Id=@Id";
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

    public static void Delete(AdminUser obj)
    {
      try
      {
        string str = "DELETE FROM AdminUsers WHERE Id=@Id";
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
