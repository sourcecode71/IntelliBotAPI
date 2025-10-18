using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliBot.Core.Models.Configuration
{
    public class CacheConfig
    {
        public bool Enabled { get; set; } = true;
        public int TimeoutInMinutes { get; set; } = 30;
    }
}
