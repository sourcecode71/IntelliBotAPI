using IntelliBot.Core.Enums;
using IntelliBot.Core.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliBot.Core.Models.Requests
{
    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public string? ConversationId { get; set; }
        public AiModel? Model { get; set; }  // Changed to nullable
        public double? Temperature { get; set; }  // Changed to nullable
        public int? MaxTokens { get; set; }  // Changed to nullable
        public string? UserId { get; set; }
        public List<MessageRequest> PreviousMessages { get; set; } = new List<MessageRequest>();
        public string? Title { get; set; }
    }

}
