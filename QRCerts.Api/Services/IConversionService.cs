using QRCerts.Api.Models;

namespace QRCerts.Api.Services
{
    public interface IConversionService
    {
        Task<ConvertResponse> ConvertToBase64Async(ConvertRequest request);
        Task<(byte[] PdfBytes, string FileName)> ConvertToStreamAsync(ConvertRequest request);

    }
}
