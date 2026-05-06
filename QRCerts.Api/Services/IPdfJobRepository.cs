public interface IPdfJobRepository
{
    Task<Guid> EnqueueAsync(PdfJob job);
    Task<PdfJob?> GetNextPendingJobAsync();
    Task<PdfJob?> GetByIdAsync(Guid jobId);
    Task MarkDoneAsync(Guid jobId, string outputPath);
    Task MarkErrorAsync(Guid jobId, string error);
}
