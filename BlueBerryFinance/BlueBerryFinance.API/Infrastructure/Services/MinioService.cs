using BlueBerryFinance.API.Infrastructure.Models;
using BlueBerryFinance.API.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace BlueBerryFinance.API.Infrastructure.Services
{
    public class MinioService : IMinioService
    {
        private readonly IMinioClient _minio;
        private readonly MinioOptions _opts;
        private readonly ILogger<MinioService> _logger;

        public MinioService(IOptions<MinioOptions> opts, ILogger<MinioService> logger)
        {
            _opts = opts.Value;
            _logger = logger;

            _minio = new MinioClient()
                .WithEndpoint(_opts.Endpoint)
                .WithCredentials(_opts.AccessKey, _opts.SecretKey)
                .WithSSL(_opts.UseSSL)
                .Build();
        }

        public async Task<string> UploadAsync(
            Stream stream,
            string fileName,
            string contentType)
        {
            await EnsureBucketAsync();

            var prefix = contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ? "images" : "statements";
            var objectName = $"{prefix}/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid()}/{fileName}";

            var args = new PutObjectArgs()
                .WithBucket(_opts.BucketName)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType);

            await _minio.PutObjectAsync(args);

            var url = $"{_opts.PublicBaseUrl.TrimEnd('/')}/{_opts.BucketName}/{objectName}";
            _logger.LogInformation("Uploaded file to MinIO: {Url}", url);
            return url;
        }

        public async Task<Stream> DownloadAsync(string objectName)
        {
            var ms = new MemoryStream();

            var args = new GetObjectArgs()
                .WithBucket(_opts.BucketName)
                .WithObject(objectName)
                .WithCallbackStream(stream => stream.CopyTo(ms));

            await _minio.GetObjectAsync(args);
            ms.Position = 0;
            return ms;
        }

        private async Task EnsureBucketAsync()
        {
            var exists = await _minio.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(_opts.BucketName));

            if (!exists)
            {
                await _minio.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(_opts.BucketName));
                _logger.LogInformation("Created MinIO bucket: {Bucket}", _opts.BucketName);
            }
        }
    }
}
