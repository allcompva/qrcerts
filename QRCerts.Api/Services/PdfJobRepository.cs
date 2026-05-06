using Dapper;
using Microsoft.Data.SqlClient;

public class PdfJobRepository : IPdfJobRepository
{
    private readonly string _cs;

    public PdfJobRepository(IConfiguration cfg)
    {
        _cs = cfg.GetConnectionString("DefaultConnection");
    }

    public async Task<Guid> EnqueueAsync(PdfJob job)
    {
        job.Id = Guid.NewGuid();

        using var cn = new SqlConnection(_cs);
        await cn.ExecuteAsync(@"
            INSERT INTO PdfJobs
            (Id,OtecId,CourseId,AlumnoIdsCsv,TipoJob,Estado,Prioridad,CreatedAt)
            VALUES
            (@Id,@OtecId,@CourseId,@AlumnoIdsCsv,@TipoJob,0,@Prioridad,SYSDATETIME())
        ", job);

        return job.Id;
    }

    public async Task<PdfJob?> GetNextPendingJobAsync()
    {
        using var cn = new SqlConnection(_cs);

        return await cn.QuerySingleOrDefaultAsync<PdfJob>(@"
            ;WITH cte AS (
                SELECT TOP 1 *
                FROM PdfJobs WITH (ROWLOCK, READPAST)
                WHERE Estado = 0
                ORDER BY Prioridad, CreatedAt
            )
            UPDATE cte
            SET Estado = 1
            OUTPUT inserted.*;
        ");
    }

    public async Task<PdfJob?> GetByIdAsync(Guid jobId)
    {
        using var cn = new SqlConnection(_cs);
        return await cn.QuerySingleOrDefaultAsync<PdfJob>(
            "SELECT * FROM PdfJobs WHERE Id = @jobId",
            new { jobId });
    }

    public async Task MarkDoneAsync(Guid jobId, string outputPath)
    {
        using var cn = new SqlConnection(_cs);
        await cn.ExecuteAsync(
            "UPDATE PdfJobs SET Estado = 2, OutputPath = @outputPath WHERE Id = @jobId",
            new { jobId, outputPath });
    }

    public async Task MarkErrorAsync(Guid jobId, string error)
    {
        using var cn = new SqlConnection(_cs);
        await cn.ExecuteAsync(
            "UPDATE PdfJobs SET Estado = 3, Error = @error WHERE Id = @jobId",
            new { jobId, error });
    }
}
