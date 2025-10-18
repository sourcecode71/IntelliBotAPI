using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliBot.Core.Models
{
    public class AiModelInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxTokens { get; set; }
        public bool SupportsVision { get; set; }
        public bool SupportsFunctionCalling { get; set; }
        public DateTime ReleaseDate { get; set; }
    }
}
