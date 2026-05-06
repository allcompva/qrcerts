using Microsoft.Data.SqlClient;
using QRCerts.Api.Models;
using System.Data;

namespace QRCerts.Api.DAL
{
    public class QuotaDAL : DALBase
    {
        // ============ OtecQuotaConfig ============

        public static OtecQuotaConfig? GetQuotaConfig(Guid otecId)
        {
            try
            {
                using var con = GetConnection();
                var cmd = con.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT Id, OtecId, QuotaActivo, CreatedAt, UpdatedAt FROM OtecQuotaConfig WHERE OtecId = @OtecId";
                cmd.Parameters.AddWithValue("@OtecId", otecId);
                con.Open();
                using var dr = cmd.ExecuteReader();
                if (!dr.Read()) return null;
                return new OtecQuotaConfig
                {
                    Id = GetGuid(dr, dr.GetOrdinal("Id")),
                    OtecId = GetGuid(dr, dr.GetOrdinal("OtecId")),
                    QuotaActivo = GetBoolean(dr, dr.GetOrdinal("QuotaActivo")),
                    CreatedAt = GetDateTime(dr, dr.GetOrdinal("CreatedAt")),
                    UpdatedAt = GetDateTime(dr, dr.GetOrdinal("UpdatedAt"))
                };
            }
            catch (Exception ex) { throw ex; }
        }

        public static void SaveQuotaConfig(Guid otecId, bool quotaActivo)
        {
            try
            {
                using var con = GetConnection();
                var cmd = con.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"
                    IF EXISTS (SELECT 1 FROM OtecQuotaConfig WHERE OtecId = @OtecId)
                        UPDATE OtecQuotaConfig SET QuotaActivo = @QuotaActivo, UpdatedAt = GETUTCDATE() WHERE OtecId = @OtecId
                    ELSE
                        INSERT INTO OtecQuotaConfig (Id, OtecId, QuotaActivo, CreatedAt, UpdatedAt)
                        VALUES (NEWID(), @OtecId, @QuotaActivo, GETUTCDATE(), GETUTCDATE())";
                cmd.Parameters.AddWithValue("@OtecId", otecId);
                cmd.Parameters.AddWithValue("@QuotaActivo", quotaActivo);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex) { throw ex; }
        }

        // ============ OrdenCompra ============

        public static OrdenCompra? GetOrdenActiva(Guid otecId)
        {
            try
            {
                using var con = GetConnection();
                var cmd = con.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"SELECT Id, OtecId, CantidadComprada, CantidadUsada,
                    FechaExpiracion, Activa, CreadaPor, Notas, CreatedAt
                    FROM OrdenCompra WHERE OtecId = @OtecId AND Activa = 1";
                cmd.Parameters.AddWithValue("@OtecId", otecId);
                con.Open();
                using var dr = cmd.ExecuteReader();
                if (!dr.Read()) return null;
                return MapOrden(dr);
            }
            catch (Exception ex) { throw ex; }
        }

        public static List<OrdenCompra> GetOrdenesByOtec(Guid otecId)
        {
            try
            {
                var list = new List<OrdenCompra>();
                using var con = GetConnection();
                var cmd = con.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"SELECT Id, OtecId, CantidadComprada, CantidadUsada,
                    FechaExpiracion, Activa, CreadaPor, Notas, CreatedAt
                    FROM OrdenCompra WHERE OtecId = @OtecId ORDER BY CreatedAt DESC";
                cmd.Parameters.AddWithValue("@OtecId", otecId);
                con.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                    list.Add(MapOrden(dr));
                return list;
            }
            catch (Exception ex) { throw ex; }
        }

        public static void CrearOrden(OrdenCompra orden, string adminUsername)
        {
            try
            {
                using var con = GetConnection();
                con.Open();
                using var tx = con.BeginTransaction();
                try
                {
                    // Desactivar orden anterior si existe
                    var cmdSupersede = con.CreateCommand();
                    cmdSupersede.Transaction = tx;
                    cmdSupersede.CommandText = @"
                        UPDATE OrdenCompra SET Activa = 0
                        OUTPUT INSERTED.Id
                        WHERE OtecId = @OtecId AND Activa = 1";
                    cmdSupersede.Parameters.AddWithValue("@OtecId", orden.OtecId);
                    var supersededId = cmdSupersede.ExecuteScalar();

                    // Registrar superseded en historial
                    if (supersededId != null && supersededId != DBNull.Value)
                    {
                        InsertHistorial(con, tx, (Guid)supersededId, orden.OtecId,
                            "SUPERSEDED", "Reemplazada por nueva orden", adminUsername);
                    }

                    // Insertar nueva orden
                    var cmdInsert = con.CreateCommand();
                    cmdInsert.Transaction = tx;
                    cmdInsert.CommandText = @"
                        INSERT INTO OrdenCompra (Id, OtecId, CantidadComprada, CantidadUsada,
                            FechaExpiracion, Activa, CreadaPor, Notas, CreatedAt)
                        VALUES (@Id, @OtecId, @CantidadComprada, 0, @FechaExpiracion,
                            1, @CreadaPor, @Notas, GETUTCDATE())";
                    cmdInsert.Parameters.AddWithValue("@Id", orden.Id);
                    cmdInsert.Parameters.AddWithValue("@OtecId", orden.OtecId);
                    cmdInsert.Parameters.AddWithValue("@CantidadComprada", orden.CantidadComprada);
                    cmdInsert.Parameters.AddWithValue("@FechaExpiracion", orden.FechaExpiracion);
                    cmdInsert.Parameters.AddWithValue("@CreadaPor", adminUsername);
                    cmdInsert.Parameters.AddWithValue("@Notas", (object?)orden.Notas ?? DBNull.Value);
                    cmdInsert.ExecuteNonQuery();

                    // Registrar creación en historial
                    InsertHistorial(con, tx, orden.Id, orden.OtecId,
                        "CREATED", $"Cantidad: {orden.CantidadComprada}, Vence: {orden.FechaExpiracion:yyyy-MM-dd}", adminUsername);

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
            catch (Exception ex) { throw ex; }
        }

        public static bool ConsumirQuota(Guid otecId, int cantidad)
        {
            try
            {
                using var con = GetConnection();
                con.Open();
                using var tx = con.BeginTransaction();
                try
                {
                    var cmd = con.CreateCommand();
                    cmd.Transaction = tx;
                    cmd.CommandText = @"
                        UPDATE OrdenCompra
                        SET CantidadUsada = CantidadUsada + @Cantidad
                        OUTPUT INSERTED.Id
                        WHERE OtecId = @OtecId AND Activa = 1
                            AND FechaExpiracion > GETUTCDATE()
                            AND (CantidadUsada + @Cantidad) <= CantidadComprada";
                    cmd.Parameters.AddWithValue("@OtecId", otecId);
                    cmd.Parameters.AddWithValue("@Cantidad", cantidad);
                    var result = cmd.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                    {
                        tx.Rollback();
                        return false;
                    }

                    InsertHistorial(con, tx, (Guid)result, otecId,
                        "QUOTA_CONSUMED", $"Consumidos: {cantidad}", "SYSTEM");

                    tx.Commit();
                    return true;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
            catch (Exception ex) { throw ex; }
        }

        // ============ Historial ============

        public static List<OrdenCompraHistorial> GetHistorialByOtec(Guid otecId)
        {
            try
            {
                var list = new List<OrdenCompraHistorial>();
                using var con = GetConnection();
                var cmd = con.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"SELECT Id, OrdenCompraId, OtecId, Evento, Detalle, CreadaPor, CreatedAt
                    FROM OrdenCompraHistorial WHERE OtecId = @OtecId ORDER BY CreatedAt DESC";
                cmd.Parameters.AddWithValue("@OtecId", otecId);
                con.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new OrdenCompraHistorial
                    {
                        Id = GetGuid(dr, dr.GetOrdinal("Id")),
                        OrdenCompraId = GetGuid(dr, dr.GetOrdinal("OrdenCompraId")),
                        OtecId = GetGuid(dr, dr.GetOrdinal("OtecId")),
                        Evento = GetString(dr, dr.GetOrdinal("Evento")),
                        Detalle = dr.IsDBNull(dr.GetOrdinal("Detalle")) ? null : dr.GetString(dr.GetOrdinal("Detalle")),
                        CreadaPor = GetString(dr, dr.GetOrdinal("CreadaPor")),
                        CreatedAt = GetDateTime(dr, dr.GetOrdinal("CreatedAt"))
                    });
                }
                return list;
            }
            catch (Exception ex) { throw ex; }
        }

        // ============ Helpers ============

        private static void InsertHistorial(SqlConnection con, SqlTransaction tx,
            Guid ordenId, Guid otecId, string evento, string? detalle, string creadaPor)
        {
            var cmd = con.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
                INSERT INTO OrdenCompraHistorial (Id, OrdenCompraId, OtecId, Evento, Detalle, CreadaPor, CreatedAt)
                VALUES (NEWID(), @OrdenCompraId, @OtecId, @Evento, @Detalle, @CreadaPor, GETUTCDATE())";
            cmd.Parameters.AddWithValue("@OrdenCompraId", ordenId);
            cmd.Parameters.AddWithValue("@OtecId", otecId);
            cmd.Parameters.AddWithValue("@Evento", evento);
            cmd.Parameters.AddWithValue("@Detalle", (object?)detalle ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreadaPor", creadaPor);
            cmd.ExecuteNonQuery();
        }

        private static OrdenCompra MapOrden(SqlDataReader dr)
        {
            return new OrdenCompra
            {
                Id = GetGuid(dr, dr.GetOrdinal("Id")),
                OtecId = GetGuid(dr, dr.GetOrdinal("OtecId")),
                CantidadComprada = GetInt32(dr, dr.GetOrdinal("CantidadComprada")),
                CantidadUsada = GetInt32(dr, dr.GetOrdinal("CantidadUsada")),
                FechaExpiracion = GetDateTime(dr, dr.GetOrdinal("FechaExpiracion")),
                Activa = GetBoolean(dr, dr.GetOrdinal("Activa")),
                CreadaPor = GetString(dr, dr.GetOrdinal("CreadaPor")),
                Notas = dr.IsDBNull(dr.GetOrdinal("Notas")) ? null : dr.GetString(dr.GetOrdinal("Notas")),
                CreatedAt = GetDateTime(dr, dr.GetOrdinal("CreatedAt"))
            };
        }
    }
}
