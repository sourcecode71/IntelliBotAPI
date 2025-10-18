using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliBot.Core.Models.Responses
{
    public class ConversationResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<MessageRequest> Messages { get; set; } = new List<MessageRequest>();
        public int TotalMessages { get; set; }
        public string Answer { get; set; } = string.Empty;

    }
}
