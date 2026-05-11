namespace BlueBerryFinance.API.Infrastructure.Services.Interfaces
{
    public interface IMinioService
    {
        Task<string> UploadAsync(
            Stream stream,
            string fileName,
            string contentType);

        Task<Stream> DownloadAsync(string objectName);
    }
}
