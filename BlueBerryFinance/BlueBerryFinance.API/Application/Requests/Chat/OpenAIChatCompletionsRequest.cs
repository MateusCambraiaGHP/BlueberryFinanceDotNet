namespace BlueBerryFinance.API.Application.Requests.Chat
{
    public class OpenAIChatCompletionsRequest
    {
        public string Model { get; set; } = string.Empty;
        public IList<OpenAIChatMessage> Messages { get; set; } = [];
        public bool Stream { get; set; } = true;
        public string? User { get; set; }
    }

    public class OpenAIChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
