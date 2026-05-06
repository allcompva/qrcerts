public class PdfJobWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public PdfJobWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IPdfJobRepository>();

            var job = await repo.GetNextPendingJobAsync();
            if (job == null)
            {
                await Task.Delay(3000, stoppingToken);
                continue;
            }

            try
            {
                // ACA TU LOGICA REAL DE GENERAR PDF + ZIP
                var zipPath = await GenerarZipAsync(job);

                await repo.MarkDoneAsync(job.Id, zipPath);
            }
            catch (Exception ex)
            {
                await repo.MarkErrorAsync(job.Id, ex.Message);
            }
        }
    }

    private Task<string> GenerarZipAsync(PdfJob job)
    {
        // stub
        var path = Path.Combine(Path.GetTempPath(), job.Id + ".zip");
        File.WriteAllText(path, "zip fake");
        return Task.FromResult(path);
    }
}
