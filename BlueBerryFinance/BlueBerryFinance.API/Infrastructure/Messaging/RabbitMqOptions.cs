namespace BlueBerryFinance.API.Infrastructure.Messaging
{
    public class RabbitMqOptions
    {
        public string Host        { get; set; } = "localhost";
        public string User        { get; set; } = "guest";
        public string Password    { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
    }
}
