    // Decompiled with JetBrains decompiler
// Type: QRCerts.Api.DAL.DALBase
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

#nullable enable
namespace QRCerts.Api.DAL
{
  public abstract class DALBase
  {
    protected static SqlConnection GetConnection()
    {
      return new SqlConnection(new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true).AddEnvironmentVariables().Build().GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."));
    }

    protected static T GetValue<T>(SqlDataReader dr, int ordinal)
    {
      return dr.IsDBNull(ordinal) ? default (T) : (T) dr.GetValue(ordinal);
    }

    protected static string GetString(SqlDataReader dr, int ordinal)
    {
      return !dr.IsDBNull(ordinal) ? dr.GetString(ordinal) : string.Empty;
    }

    protected static int GetInt32(SqlDataReader dr, int ordinal)
    {
      return !dr.IsDBNull(ordinal) ? dr.GetInt32(ordinal) : 0;
    }

    protected static Guid GetGuid(SqlDataReader dr, int ordinal)
    {
      return !dr.IsDBNull(ordinal) ? dr.GetGuid(ordinal) : Guid.Empty;
    }

    protected static DateTime GetDateTime(SqlDataReader dr, int ordinal)
    {
      return !dr.IsDBNull(ordinal) ? dr.GetDateTime(ordinal) : DateTime.MinValue;
    }

    protected static DateTime? GetNullableDateTime(SqlDataReader dr, int ordinal)
    {
      return !dr.IsDBNull(ordinal) ? new DateTime?(dr.GetDateTime(ordinal)) : new DateTime?();
    }

    protected static bool GetBoolean(SqlDataReader dr, int ordinal)
    {
      return !dr.IsDBNull(ordinal) && dr.GetBoolean(ordinal);
    }

    protected static byte GetByte(SqlDataReader dr, int ordinal)
    {
      return !dr.IsDBNull(ordinal) ? dr.GetByte(ordinal) : (byte) 0;
    }

    protected static Decimal GetDecimal(SqlDataReader dr, int ordinal)
    {
      return !dr.IsDBNull(ordinal) ? dr.GetDecimal(ordinal) : 0M;
    }
  }
}
