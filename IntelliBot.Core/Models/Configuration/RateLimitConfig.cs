using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliBot.Core.Models.Configuration
{
    internal class RateLimitConfig
    {
        public int RequestsPerMinute { get; set; } = 10;
        public int RequestsPerHour { get; set; } = 100;
    }
}
