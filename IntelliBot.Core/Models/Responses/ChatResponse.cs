using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliBot.Core.Models.Responses
{
    public class ChatResponse
    {
        public string Answer { get; set; } = string.Empty;
        public string ConversationId { get; set; } = string.Empty;
        public int TokensUsed { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public string ModelUsed { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
