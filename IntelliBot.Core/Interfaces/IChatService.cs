using IntelliBot.Core.Models;
using IntelliBot.Core.Models.Requests;
using IntelliBot.Core.Models.Responses;

namespace IntelliBot.Core.Interfaces
{
    public interface IChatService
    {
        /// <summary>
        /// Sends a message to the AI model and returns the response
        /// </summary>
        /// <param name="request">The chat request containing message and configuration</param>
        /// <returns>Chat response from the AI model</returns>
        Task<ChatResponse> SendMessageAsync(ChatRequest request);

        /// <summary>
        /// Sends a message with streaming response for real-time updates
        /// </summary>
        /// <param name="request">The chat request</param>
        /// <param name="onTokenReceived">Callback for each token received</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the streaming operation</returns>
        Task StreamMessageAsync(ChatRequest request, Func<string, Task> onTokenReceived, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets conversation history by conversation ID
        /// </summary>
        /// <param name="conversationId">Unique identifier for the conversation</param>
        /// <returns>Conversation with all messages</returns>
        Task<ConversationResponse> GetConversationAsync(string conversationId);

        /// <summary>
        /// Gets all conversations for a user (optional user filtering)
        /// </summary>
        /// <param name="userId">Optional user identifier</param>
        /// <param name="page">Page number for pagination</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>Paginated list of conversations</returns>
        Task<List<ConversationResponse>> GetConversationsAsync(string? userId = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// Creates a new conversation
        /// </summary>
        /// <param name="request">Conversation creation request</param>
        /// <returns>Created conversation</returns>
        Task<ConversationResponse> CreateConversationAsync(ChatRequest request);

        /// <summary>
        /// Deletes a conversation and all its messages
        /// </summary>
        /// <param name="conversationId">Conversation identifier</param>
        /// <returns>True if deletion was successful</returns>
        Task<bool> DeleteConversationAsync(string conversationId);

        /// <summary>
        /// Updates conversation title
        /// </summary>
        /// <param name="conversationId">Conversation identifier</param>
        /// <param name="title">New title for the conversation</param>
        /// <returns>Updated conversation</returns>
        Task<ConversationResponse> UpdateConversationTitleAsync(string conversationId, string title);

        /// <summary>
        /// Gets usage statistics for a given period
        /// </summary>
        /// <param name="fromDate">Start date for the period</param>
        /// <param name="toDate">End date for the period</param>
        /// <param name="userId">Optional user filter</param>
        /// <returns>Usage statistics</returns>
        Task<UsageStatsResponse> GetUsageStatisticsAsync(DateTime fromDate, DateTime toDate, string? userId = null);

        /// <summary>
        /// Gets available AI models with their capabilities
        /// </summary>
        /// <returns>List of available models</returns>
        Task<List<AiModelInfo>> GetAvailableModelsAsync();

        /// <summary>
        /// Validates if the API key and configuration are working
        /// </summary>
        /// <returns>True if the service is properly configured</returns>
        Task<bool> ValidateConfigurationAsync();
    }
}