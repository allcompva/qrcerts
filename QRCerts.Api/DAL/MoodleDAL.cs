using Microsoft.Data.SqlClient;
using QRCerts.Api.Models;

namespace QRCerts.Api.DAL
{
    public class MoodleDAL : DALBase
    {
        #region MoodleConfig

        public static MoodleConfig? GetConfigByOtecId(Guid otecId)
        {
            using var conn = GetConnection();
            conn.Open();

            var sql = @"SELECT Id, OtecId, MoodleUrl, Token, Activo, UltimaConexionExitosa, CreatedAt, UpdatedAt
                        FROM MoodleConfigs WHERE OtecId = @OtecId";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@OtecId", otecId);

            using var dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                return new MoodleConfig
                {
                    Id = GetGuid(dr, 0),
                    OtecId = GetGuid(dr, 1),
                    MoodleUrl = GetString(dr, 2),
                    Token = GetString(dr, 3),
                    Activo = GetBoolean(dr, 4),
                    UltimaConexionExitosa = GetNullableDateTime(dr, 5),
                    CreatedAt = GetDateTime(dr, 6),
                    UpdatedAt = GetDateTime(dr, 7)
                };
            }

            return null;
        }

        public static void SaveConfig(MoodleConfig config)
        {
            using var conn = GetConnection();
            conn.Open();

            // Check if exists
            var existingConfig = GetConfigByOtecId(config.OtecId);

            if (existingConfig == null)
            {
                var sql = @"INSERT INTO MoodleConfigs (Id, OtecId, MoodleUrl, Token, Activo, UltimaConexionExitosa, CreatedAt, UpdatedAt)
                            VALUES (@Id, @OtecId, @MoodleUrl, @Token, @Activo, @UltimaConexionExitosa, @CreatedAt, @UpdatedAt)";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", config.Id);
                cmd.Parameters.AddWithValue("@OtecId", config.OtecId);
                cmd.Parameters.AddWithValue("@MoodleUrl", config.MoodleUrl);
                cmd.Parameters.AddWithValue("@Token", config.Token);
                cmd.Parameters.AddWithValue("@Activo", config.Activo);
                cmd.Parameters.AddWithValue("@UltimaConexionExitosa", (object?)config.UltimaConexionExitosa ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedAt", config.CreatedAt);
                cmd.Parameters.AddWithValue("@UpdatedAt", config.UpdatedAt);
                cmd.ExecuteNonQuery();
            }
            else
            {
                var sql = @"UPDATE MoodleConfigs SET
                            MoodleUrl = @MoodleUrl,
                            Token = @Token,
                            Activo = @Activo,
                            UltimaConexionExitosa = @UltimaConexionExitosa,
                            UpdatedAt = @UpdatedAt
                            WHERE OtecId = @OtecId";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@OtecId", config.OtecId);
                cmd.Parameters.AddWithValue("@MoodleUrl", config.MoodleUrl);
                cmd.Parameters.AddWithValue("@Token", config.Token);
                cmd.Parameters.AddWithValue("@Activo", config.Activo);
                cmd.Parameters.AddWithValue("@UltimaConexionExitosa", (object?)config.UltimaConexionExitosa ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateUltimaConexion(Guid otecId)
        {
            using var conn = GetConnection();
            conn.Open();

            var sql = @"UPDATE MoodleConfigs SET UltimaConexionExitosa = @Fecha, UpdatedAt = @Fecha WHERE OtecId = @OtecId";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@OtecId", otecId);
            cmd.Parameters.AddWithValue("@Fecha", DateTime.UtcNow);
            cmd.ExecuteNonQuery();
        }

        #endregion

        #region MoodleCursoImportado

        public static MoodleCursoImportado? GetCursoImportado(Guid otecId, int moodleCourseId)
        {
            using var conn = GetConnection();
            conn.Open();

            var sql = @"SELECT Id, OtecId, MoodleCourseId, NombreCurso, ShortName, Categoria, CursoLocalId, CantidadAlumnos, UltimaSync, CreatedAt
                        FROM MoodleCursosImportados WHERE OtecId = @OtecId AND MoodleCourseId = @MoodleCourseId";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@OtecId", otecId);
            cmd.Parameters.AddWithValue("@MoodleCourseId", moodleCourseId);

            using var dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                return MapCursoImportado(dr);
            }

            return null;
        }

        public static List<MoodleCursoImportado> GetCursosImportadosByOtec(Guid otecId)
        {
            var result = new List<MoodleCursoImportado>();

            using var conn = GetConnection();
            conn.Open();

            var sql = @"SELECT Id, OtecId, MoodleCourseId, NombreCurso, ShortName, Categoria, CursoLocalId, CantidadAlumnos, UltimaSync, CreatedAt
                        FROM MoodleCursosImportados WHERE OtecId = @OtecId ORDER BY UltimaSync DESC";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@OtecId", otecId);

            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                result.Add(MapCursoImportado(dr));
            }

            return result;
        }

        public static void SaveCursoImportado(MoodleCursoImportado curso)
        {
            using var conn = GetConnection();
            conn.Open();

            var existing = GetCursoImportado(curso.OtecId, curso.MoodleCourseId);

            if (existing == null)
            {
                var sql = @"INSERT INTO MoodleCursosImportados (Id, OtecId, MoodleCourseId, NombreCurso, ShortName, Categoria, CursoLocalId, CantidadAlumnos, UltimaSync, CreatedAt)
                            VALUES (@Id, @OtecId, @MoodleCourseId, @NombreCurso, @ShortName, @Categoria, @CursoLocalId, @CantidadAlumnos, @UltimaSync, @CreatedAt)";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", curso.Id);
                cmd.Parameters.AddWithValue("@OtecId", curso.OtecId);
                cmd.Parameters.AddWithValue("@MoodleCourseId", curso.MoodleCourseId);
                cmd.Parameters.AddWithValue("@NombreCurso", curso.NombreCurso);
                cmd.Parameters.AddWithValue("@ShortName", curso.ShortName);
                cmd.Parameters.AddWithValue("@Categoria", curso.Categoria);
                cmd.Parameters.AddWithValue("@CursoLocalId", (object?)curso.CursoLocalId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CantidadAlumnos", curso.CantidadAlumnos);
                cmd.Parameters.AddWithValue("@UltimaSync", curso.UltimaSync);
                cmd.Parameters.AddWithValue("@CreatedAt", curso.CreatedAt);
                cmd.ExecuteNonQuery();
            }
            else
            {
                var sql = @"UPDATE MoodleCursosImportados SET
                            NombreCurso = @NombreCurso,
                            ShortName = @ShortName,
                            Categoria = @Categoria,
                            CursoLocalId = @CursoLocalId,
                            CantidadAlumnos = @CantidadAlumnos,
                            UltimaSync = @UltimaSync
                            WHERE OtecId = @OtecId AND MoodleCourseId = @MoodleCourseId";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@OtecId", curso.OtecId);
                cmd.Parameters.AddWithValue("@MoodleCourseId", curso.MoodleCourseId);
                cmd.Parameters.AddWithValue("@NombreCurso", curso.NombreCurso);
                cmd.Parameters.AddWithValue("@ShortName", curso.ShortName);
                cmd.Parameters.AddWithValue("@Categoria", curso.Categoria);
                cmd.Parameters.AddWithValue("@CursoLocalId", (object?)curso.CursoLocalId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CantidadAlumnos", curso.CantidadAlumnos);
                cmd.Parameters.AddWithValue("@UltimaSync", DateTime.UtcNow);
                cmd.ExecuteNonQuery();
            }
        }

        private static MoodleCursoImportado MapCursoImportado(SqlDataReader dr)
        {
            var cursoLocalId = dr.IsDBNull(6) ? (Guid?)null : GetGuid(dr, 6);
            return new MoodleCursoImportado
            {
                Id = GetGuid(dr, 0),
                OtecId = GetGuid(dr, 1),
                MoodleCourseId = GetInt32(dr, 2),
                NombreCurso = GetString(dr, 3),
                ShortName = GetString(dr, 4),
                Categoria = GetString(dr, 5),
                CursoLocalId = cursoLocalId,
                CantidadAlumnos = GetInt32(dr, 7),
                UltimaSync = GetDateTime(dr, 8),
                CreatedAt = GetDateTime(dr, 9)
            };
        }

        #endregion

        #region MoodleFieldMapping

        public static List<MoodleFieldMapping> GetFieldMappingsByCurso(Guid cursoId)
        {
            var result = new List<MoodleFieldMapping>();

            using var conn = GetConnection();
            conn.Open();

            var sql = @"SELECT Id, CursoId, CampoMoodle, VariableCertificado, Orden, EsObligatorio, CreatedAt
                        FROM MoodleFieldMappings WHERE CursoId = @CursoId ORDER BY Orden";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CursoId", cursoId);

            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                result.Add(new MoodleFieldMapping
                {
                    Id = GetGuid(dr, 0),
                    CursoId = GetGuid(dr, 1),
                    CampoMoodle = GetString(dr, 2),
                    VariableCertificado = GetString(dr, 3),
                    Orden = GetInt32(dr, 4),
                    EsObligatorio = GetBoolean(dr, 5),
                    CreatedAt = GetDateTime(dr, 6)
                });
            }

            return result;
        }

        public static void SaveFieldMappings(Guid cursoId, List<MoodleFieldMapping> mappings)
        {
            using var conn = GetConnection();
            conn.Open();

            // Delete existing
            var deleteSql = "DELETE FROM MoodleFieldMappings WHERE CursoId = @CursoId";
            using (var deleteCmd = new SqlCommand(deleteSql, conn))
            {
                deleteCmd.Parameters.AddWithValue("@CursoId", cursoId);
                deleteCmd.ExecuteNonQuery();
            }

            // Insert new
            var insertSql = @"INSERT INTO MoodleFieldMappings (Id, CursoId, CampoMoodle, VariableCertificado, Orden, EsObligatorio, CreatedAt)
                              VALUES (@Id, @CursoId, @CampoMoodle, @VariableCertificado, @Orden, @EsObligatorio, @CreatedAt)";

            foreach (var mapping in mappings)
            {
                using var cmd = new SqlCommand(insertSql, conn);
                cmd.Parameters.AddWithValue("@Id", mapping.Id == Guid.Empty ? Guid.NewGuid() : mapping.Id);
                cmd.Parameters.AddWithValue("@CursoId", cursoId);
                cmd.Parameters.AddWithValue("@CampoMoodle", mapping.CampoMoodle);
                cmd.Parameters.AddWithValue("@VariableCertificado", mapping.VariableCertificado);
                cmd.Parameters.AddWithValue("@Orden", mapping.Orden);
                cmd.Parameters.AddWithValue("@EsObligatorio", mapping.EsObligatorio);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region SQL Scripts para crear tablas

        public static string GetCreateTablesScript()
        {
            return @"
-- Tabla MoodleConfigs
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MoodleConfigs')
BEGIN
    CREATE TABLE MoodleConfigs (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        OtecId UNIQUEIDENTIFIER NOT NULL,
        MoodleUrl NVARCHAR(500) NOT NULL,
        Token NVARCHAR(500) NOT NULL,
        Activo BIT NOT NULL DEFAULT 1,
        UltimaConexionExitosa DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_MoodleConfigs_Otecs FOREIGN KEY (OtecId) REFERENCES Otecs(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_MoodleConfigs_OtecId UNIQUE (OtecId)
    );
END

-- Tabla MoodleCursosImportados
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MoodleCursosImportados')
BEGIN
    CREATE TABLE MoodleCursosImportados (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        OtecId UNIQUEIDENTIFIER NOT NULL,
        MoodleCourseId INT NOT NULL,
        NombreCurso NVARCHAR(500) NOT NULL,
        ShortName NVARCHAR(200) NOT NULL DEFAULT '',
        Categoria NVARCHAR(200) NOT NULL DEFAULT '',
        CursoLocalId UNIQUEIDENTIFIER NULL,
        CantidadAlumnos INT NOT NULL DEFAULT 0,
        UltimaSync DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_MoodleCursosImportados_Otecs FOREIGN KEY (OtecId) REFERENCES Otecs(Id) ON DELETE CASCADE,
        CONSTRAINT FK_MoodleCursosImportados_Cursos FOREIGN KEY (CursoLocalId) REFERENCES Cursos(Id) ON DELETE SET NULL,
        CONSTRAINT UQ_MoodleCursosImportados_OtecCourse UNIQUE (OtecId, MoodleCourseId)
    );
END

-- Tabla MoodleFieldMappings
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MoodleFieldMappings')
BEGIN
    CREATE TABLE MoodleFieldMappings (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        CursoId UNIQUEIDENTIFIER NOT NULL,
        CampoMoodle NVARCHAR(200) NOT NULL,
        VariableCertificado NVARCHAR(200) NOT NULL,
        Orden INT NOT NULL DEFAULT 0,
        EsObligatorio BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_MoodleFieldMappings_Cursos FOREIGN KEY (CursoId) REFERENCES Cursos(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_MoodleFieldMappings_CursoCampo UNIQUE (CursoId, CampoMoodle)
    );
END
";
        }

        #endregion
    }
}
