namespace BlueBerryFinance.API.Infrastructure.Models
{
    public class MinioOptions
    {
        public string Endpoint { get; set; } = "localhost:9000";
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string BucketName { get; set; } = "blueberry-files";
        public bool UseSSL { get; set; } = false;
        public string PublicBaseUrl { get; set; } = "http://localhost:9000";
    }
}
