using IntelliBot.Core.Interfaces;
using IntelliBot.Core.Models.Entities;
using IntelliBot.Infrastructure.Clients;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace IntelliBot.Infrastructure.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        // In-memory storage for development - replace with actual database in production
        private static readonly List<Conversation> _conversations = new List<Conversation>();
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<ConversationRepository> _logger;

        public ConversationRepository(IMemoryCache memoryCache, ILogger<ConversationRepository> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<Conversation?> GetByIdAsync(string id)
        {
            try
            {
                var cacheKey = $"conversation_{id}";
                if (_memoryCache.TryGetValue(cacheKey, out Conversation? cachedConversation))
                {
                    return cachedConversation;
                }

                var conversation = _conversations.FirstOrDefault(c => c.Id == id);

                if (conversation != null)
                {
                    _memoryCache.Set(cacheKey, conversation, TimeSpan.FromMinutes(30));
                }

                return await Task.FromResult(conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversation {ConversationId}", id);
                throw;
            }
        }

        public async Task<List<Conversation>> GetAllAsync(string? userId = null, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _conversations.AsQueryable();

                if (!string.IsNullOrEmpty(userId))
                {
                    query = query.Where(c => c.UserId == userId);
                }

                var conversations = query
                    .OrderByDescending(c => c.UpdatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return await Task.FromResult(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversations for user {UserId}", userId);
                throw;
            }
        }

        public async Task AddAsync(Conversation conversation)
        {
            try
            {
                // Check if conversation already exists
                if (_conversations.Any(c => c.Id == conversation.Id))
                {
                    throw new InvalidOperationException($"Conversation with ID {conversation.Id} already exists");
                }

                _conversations.Add(conversation);

                // Update cache
                var cacheKey = $"conversation_{conversation.Id}";
                _memoryCache.Set(cacheKey, conversation, TimeSpan.FromMinutes(30));

                _logger.LogInformation("Added new conversation {ConversationId} with {MessageCount} messages",
                    conversation.Id, conversation.Messages.Count);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding conversation {ConversationId}", conversation.Id);
                throw;
            }
        }

        public async Task UpdateAsync(Conversation conversation)
        {
            try
            {
                var existingConversation = _conversations.FirstOrDefault(c => c.Id == conversation.Id);
                if (existingConversation == null)
                {
                    throw new ArgumentException($"Conversation with ID {conversation.Id} not found");
                }

                // Update properties
                existingConversation.Title = conversation.Title;
                existingConversation.UserId = conversation.UserId;
                existingConversation.UpdatedAt = DateTime.UtcNow;
                existingConversation.Messages = conversation.Messages;

                // Update cache
                var cacheKey = $"conversation_{conversation.Id}";
                _memoryCache.Set(cacheKey, existingConversation, TimeSpan.FromMinutes(30));

                _logger.LogInformation("Updated conversation {ConversationId}", conversation.Id);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating conversation {ConversationId}", conversation.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var conversation = _conversations.FirstOrDefault(c => c.Id == id);
                if (conversation == null)
                {
                    return false;
                }

                _conversations.Remove(conversation);

                // Remove from cache
                var cacheKey = $"conversation_{id}";
                _memoryCache.Remove(cacheKey);

                _logger.LogInformation("Deleted conversation {ConversationId}", id);

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting conversation {ConversationId}", id);
                throw;
            }
        }

        public async Task<UsageStatistics> GetUsageStatisticsAsync(DateTime fromDate, DateTime toDate, string? userId = null)
        {
            try
            {
                var query = _conversations.AsQueryable();

                // Filter by date range
                query = query.Where(c => c.UpdatedAt >= fromDate && c.UpdatedAt <= toDate);

                if (!string.IsNullOrEmpty(userId))
                {
                    query = query.Where(c => c.UserId == userId);
                }

                var conversations = query.ToList();

                var stats = new UsageStatistics
                {
                    TotalRequests = conversations.Sum(c => c.Messages.Count(m => m.Role == Core.Enums.MessageRole.User)),
                    TotalTokens = conversations.Sum(c => c.Messages.Sum(m => m.TokensUsed)),
                    RequestsPerModel = conversations
                        .SelectMany(c => c.Messages)
                        .GroupBy(m => m.ModelUsed)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                _logger.LogDebug(
                    "Generated usage statistics: {Requests} requests, {Tokens} tokens from {FromDate} to {ToDate}",
                    stats.TotalRequests, stats.TotalTokens, fromDate, toDate);

                return await Task.FromResult(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating usage statistics from {FromDate} to {ToDate}", fromDate, toDate);
                throw;
            }
        }

        public async Task<Conversation?> GetMostRecentByUserIdAsync(string userId)
        {
            try
            {
                // Only return for userId "126" as requested
                if (userId != "126")
                    return await Task.FromResult<Conversation?>(null);

                var conversation = _conversations
                    .Where(c => c.UserId == "126")
                    .OrderByDescending(c => c.UpdatedAt)
                    .FirstOrDefault();

                return await Task.FromResult(conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving most recent conversation for user {UserId}", userId);
                throw;
            }
        }
    }
}