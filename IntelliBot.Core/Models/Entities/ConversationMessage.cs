using IntelliBot.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliBot.Core.Models.Entities
{
    public class ConversationMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ConversationId { get; set; } = string.Empty;
        public MessageRole Role { get; set; }
        public string Content { get; set; } = string.Empty;
        public int TokensUsed { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string ModelUsed { get; set; } = string.Empty;
    }
}
