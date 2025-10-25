namespace IntelliBot.Core.Models.Configuration
{
    public class OpenAIConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string? Organization { get; set; }
        public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";
        public string DefaultModel { get; set; } = "openai/gpt-4o";
        public int MaxTokens { get; set; } = 1000;
        public double Temperature { get; set; } = 0.7;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    }
}