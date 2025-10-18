using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliBot.Core.Models.Configuration
{
    public class OpenAIConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string? Organization { get; set; }
        public string BaseUrl { get; set; } = "https://api.openai.com/v1/";
        public string DefaultModel { get; set; } = "gpt-3.5-turbo";
        public int MaxTokens { get; set; } = 1000;
        public double Temperature { get; set; } = 0.7;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    }
}
