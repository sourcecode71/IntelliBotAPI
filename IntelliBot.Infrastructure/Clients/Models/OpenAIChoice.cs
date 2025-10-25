using System.Text.Json.Serialization;

namespace IntelliBot.Infrastructure.Clients.Models
{
    public class OpenAIChoice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("message")]
        public OpenAIMessage Message { get; set; } = new OpenAIMessage();

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; } = string.Empty;
    }
}