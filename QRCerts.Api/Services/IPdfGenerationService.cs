namespace QRCerts.Api.Services
{
    public interface IPdfGenerationService
    {
        Task<string> GenerateZipAsync(
            Guid courseId,
            string alumnosCsv,
            CancellationToken ct);
    }

}
