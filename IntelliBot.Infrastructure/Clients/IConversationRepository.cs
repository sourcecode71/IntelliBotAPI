using IntelliBot.Core.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliBot.Infrastructure.Clients
{
    public interface IConversationRepository
    {
        Task<Conversation?> GetByIdAsync(string id);
        Task<List<Conversation>> GetAllAsync(string? userId = null, int page = 1, int pageSize = 20);
        Task AddAsync(Conversation conversation);
        Task UpdateAsync(Conversation conversation);
        Task<bool> DeleteAsync(string id);
        Task<UsageStatistics> GetUsageStatisticsAsync(DateTime fromDate, DateTime toDate, string? userId = null);
        Task<Conversation?> GetMostRecentByUserIdAsync(string userId);
    }

    public class UsageStatistics
    {
        public int TotalRequests { get; set; }
        public int TotalTokens { get; set; }
        public Dictionary<string, int> RequestsPerModel { get; set; } = new();
    }
}