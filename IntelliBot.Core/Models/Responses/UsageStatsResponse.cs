using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliBot.Core.Models.Responses
{
    public class UsageStatsResponse
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalRequests { get; set; }
        public int TotalTokensUsed { get; set; }
        public decimal EstimatedCost { get; set; }
        public Dictionary<string, int> RequestsPerModel { get; set; } = new();
    }
}
