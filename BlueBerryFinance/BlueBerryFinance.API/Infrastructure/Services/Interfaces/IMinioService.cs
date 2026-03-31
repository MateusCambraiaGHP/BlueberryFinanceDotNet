namespace BlueBerryFinance.API.Infrastructure.Services.Interfaces
{
    public interface IMinioService
    {
        /// <summary>Uploads a file and returns its public URL.</summary>
        Task<string> UploadAsync(
            Stream stream,
            string fileName,
            string contentType,
            CancellationToken ct = default);

        /// <summary>Downloads a file as a stream.</summary>
        Task<Stream> DownloadAsync(string objectName, CancellationToken ct = default);
    }
}
