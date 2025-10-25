using System.Text.Json.Serialization;

namespace IntelliBot.Infrastructure.Clients.Models
{
    public class OpenAIResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("choices")]
        public List<OpenAIChoice> Choices { get; set; } = new List<OpenAIChoice>();

        [JsonPropertyName("usage")]
        public OpenAIUsage Usage { get; set; } = new OpenAIUsage();
    }
}