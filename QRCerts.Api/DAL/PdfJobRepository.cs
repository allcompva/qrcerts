using Microsoft.Data.SqlClient;

namespace QRCerts.Api.DAL
{
    public class PdfJobRepository
    {
        private readonly string _connectionString;

        public PdfJobRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("Default");
        }

        public async Task<Guid> EnqueueAsync(
            Guid empresaId,
            Guid courseId,
            string alumnoIdsCsv,
            int tipo,
            int prioridad = 100)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
        INSERT INTO PDF_JOB
        (Id, EmpresaId, CourseId, AlumnoIdsCsv, TipoJob, Estado, Prioridad, CreatedAt)
        VALUES
        (NEWID(), @EmpresaId, @CourseId, @AlumnoIdsCsv, @TipoJob, 0, @Prioridad, SYSDATETIME())
        OUTPUT inserted.Id;
        ";

            cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
            cmd.Parameters.AddWithValue("@CourseId", courseId);
            cmd.Parameters.AddWithValue("@AlumnoIdsCsv", alumnoIdsCsv);
            cmd.Parameters.AddWithValue("@TipoJob", (int)tipo);
            cmd.Parameters.AddWithValue("@Prioridad", prioridad);

            await conn.OpenAsync();
            return (Guid)await cmd.ExecuteScalarAsync();
        }

        public async Task<PdfJob?> GetNextPendingJobAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
        ;WITH cte AS (
            SELECT TOP 1 *
            FROM PDF_JOB WITH (ROWLOCK, READPAST)
            WHERE Estado = 0
            ORDER BY Prioridad, CreatedAt
        )
        UPDATE cte
        SET Estado = 1,
            StartedAt = SYSDATETIME()
        OUTPUT inserted.*;
        ";

            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            if (!reader.Read()) return null;

            return Map(reader);
        }

        public async Task MarkDoneAsync(Guid jobId, string? zipPath)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
        UPDATE PDF_JOB
        SET Estado = 2,
            FinishedAt = SYSDATETIME(),
            ZipPath = @ZipPath
        WHERE Id = @Id;
        ";

            cmd.Parameters.AddWithValue("@Id", jobId);
            cmd.Parameters.AddWithValue("@ZipPath", (object?)zipPath ?? DBNull.Value);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task MarkErrorAsync(Guid jobId, string error)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
        UPDATE PDF_JOB
        SET Estado = 3,
            ErrorMessage = @Error,
            FinishedAt = SYSDATETIME()
        WHERE Id = @Id;
        ";

            cmd.Parameters.AddWithValue("@Id", jobId);
            cmd.Parameters.AddWithValue("@Error", error);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        private PdfJob Map(SqlDataReader r)
        {
            return new PdfJob
            {
                Id = r.GetGuid(r.GetOrdinal("Id")),
                
                OtecId= r.GetGuid(r.GetOrdinal("EmpresaId")),
                CourseId = r.GetGuid(r.GetOrdinal("CourseId")),
                AlumnoIdsCsv = r.GetString(r.GetOrdinal("AlumnoIdsCsv")),
                TipoJob = r.GetInt32(r.GetOrdinal("TipoJob")),
                Estado = r.GetInt32(r.GetOrdinal("Estado")),
                Prioridad = r.GetInt32(r.GetOrdinal("Prioridad"))
            };
        }
    }

}
