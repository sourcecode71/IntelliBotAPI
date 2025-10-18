using IntelliBot.Core.Enums;
using IntelliBot.Core.Interfaces;
using IntelliBot.Core.Models;
using IntelliBot.Core.Models.Requests;
using IntelliBot.Core.Models.Responses;
using IntelliBot.Infrastructure.Clients;
using IntelliBot.Infrastructure.Clients.Models;
using IntelliBot.Infrastructure.Repositories;
using log4net.Core;
using Microsoft.Extensions.Logging;


namespace IntelliBot.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IOpenAIClient _openAIClient;
        private readonly IConversationRepository _conversationRepository;
        private readonly ILogger<ChatService> _logger;
        private readonly ICacheService _cacheService;

        // Update the constructor parameter type to match the interface IConversationRepository
        public ChatService(
            IOpenAIClient openAIClient,
            IConversationRepository conversationRepository, // Change type to IConversationRepository
            ICacheService cacheService,
            ILogger<ChatService> logger)
        {
            _openAIClient = openAIClient;
            _conversationRepository = conversationRepository; // No cast needed as types now match
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<ChatResponse> SendMessageAsync(ChatRequest request)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                // Check cache for similar requests (optional optimization)
                var cacheKey = $"chat_{request.Model}_{request.Message.GetHashCode()}";
                if (await _cacheService.ExistsAsync(cacheKey))
                {
                    var cachedResponse = await _cacheService.GetAsync<ChatResponse>(cacheKey);
                    if (cachedResponse != null)
                    {
                        _logger.LogInformation("Returning cached response for message");
                        return cachedResponse;
                    }
                }

                // Convert to OpenAI request
                var openAIRequest = OpenAIMapper.ToOpenAIRequest(request);

                // Call OpenAI API
                var openAIResponse = await _openAIClient.GetChatCompletionAsync(openAIRequest);

                var processingTime = DateTime.UtcNow - startTime;

                // Convert to our response
                var conversationId = request.ConversationId ?? Guid.NewGuid().ToString();
                var response = OpenAIMapper.ToChatResponse(openAIResponse, conversationId, processingTime);

                // Save conversation to database
                await SaveConversationAsync(request, response);

                // Cache the response
                await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(30));

                _logger.LogInformation("Chat request processed in {ProcessingTimeMs}ms with {Tokens} tokens",
                    processingTime.TotalMilliseconds, response.TokensUsed);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing chat request");
                throw;
            }
        }

        public async Task StreamMessageAsync(ChatRequest request, Func<string, Task> onTokenReceived, CancellationToken cancellationToken = default)
        {
            try
            {
                var openAIRequest = OpenAIMapper.ToOpenAIRequest(request);
                openAIRequest.Stream = true;

                await _openAIClient.StreamChatCompletionAsync(openAIRequest, onTokenReceived, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during streaming chat request");
                throw;
            }
        }

        public async Task<ConversationResponse> GetConversationAsync(string conversationId)
        {
            var conversation = await _conversationRepository.GetByIdAsync(conversationId);
            if (conversation == null)
            {
                throw new ArgumentException($"Conversation with ID {conversationId} not found");
            }

            return MapToConversationResponse(conversation);
        }

        public async Task<List<ConversationResponse>> GetConversationsAsync(string? userId = null, int page = 1, int pageSize = 20)
        {
            var conversations = await _conversationRepository.GetAllAsync(userId, page, pageSize);
            return conversations.Select(MapToConversationResponse).ToList();
        }

        public async Task<ConversationResponse> CreateConversationAsync(ChatRequest request)
        {
            var conversation = new Core.Models.Entities.Conversation
            {
                Id = Guid.NewGuid().ToString(),
                Title = GenerateConversationTitle(request.Message),
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _conversationRepository.AddAsync(conversation);

            return new ConversationResponse
            {
                Id = conversation.Id,
                Title = conversation.Title,
                CreatedAt = conversation.CreatedAt,
                UpdatedAt = conversation.UpdatedAt,
                Messages = new List<MessageRequest>(),
                TotalMessages = 0
            };
        }

        public async Task<bool> DeleteConversationAsync(string conversationId)
        {
            return await _conversationRepository.DeleteAsync(conversationId);
        }

        public async Task<ConversationResponse> UpdateConversationTitleAsync(string conversationId, string title)
        {
            var conversation = await _conversationRepository.GetByIdAsync(conversationId);
            if (conversation == null)
            {
                throw new ArgumentException($"Conversation with ID {conversationId} not found");
            }

            conversation.Title = title;
            conversation.UpdatedAt = DateTime.UtcNow;

            await _conversationRepository.UpdateAsync(conversation);

            return MapToConversationResponse(conversation);
        }

        public async Task<UsageStatsResponse> GetUsageStatisticsAsync(DateTime fromDate, DateTime toDate, string? userId = null)
        {
            var stats = await _conversationRepository.GetUsageStatisticsAsync(fromDate, toDate, userId);

            return new UsageStatsResponse
            {
                PeriodStart = fromDate,
                PeriodEnd = toDate,
                TotalRequests = stats.TotalRequests,
                TotalTokensUsed = stats.TotalTokens,
                EstimatedCost = CalculateEstimatedCost(stats.TotalTokens),
                RequestsPerModel = stats.RequestsPerModel
            };
        }

        public async Task<List<AiModelInfo>> GetAvailableModelsAsync()
        {
            return new List<AiModelInfo>
            {
                new AiModelInfo
                {
                    Id = "gpt-3.5-turbo",
                    Name = "GPT-3.5 Turbo",
                    Description = "Fast and cost-effective for most tasks",
                    MaxTokens = 4096,
                    SupportsVision = false,
                    SupportsFunctionCalling = true,
                    ReleaseDate = new DateTime(2022, 3, 15)
                },
                new AiModelInfo
                {
                    Id = "gpt-4",
                    Name = "GPT-4",
                    Description = "More capable than GPT-3.5, better reasoning",
                    MaxTokens = 8192,
                    SupportsVision = false,
                    SupportsFunctionCalling = true,
                    ReleaseDate = new DateTime(2023, 3, 14)
                },
                new AiModelInfo
                {
                    Id = "gpt-4-turbo",
                    Name = "GPT-4 Turbo",
                    Description = "Latest GPT-4 model with improved capabilities",
                    MaxTokens = 128000,
                    SupportsVision = true,
                    SupportsFunctionCalling = true,
                    ReleaseDate = new DateTime(2023, 11, 6)
                },
                new AiModelInfo
                {
                    Id = "gpt-4o",
                    Name = "GPT-4o",
                    Description = "Our most advanced model, faster and more capable",
                    MaxTokens = 128000,
                    SupportsVision = true,
                    SupportsFunctionCalling = true,
                    ReleaseDate = new DateTime(2024, 5, 13)
                }
            };
        }

        public async Task<bool> ValidateConfigurationAsync()
        {
            try
            {
                // Send a simple test request to verify configuration
                var testRequest = new ChatRequest
                {
                    Message = "Hello, please respond with 'OK' if you can read this.",
                    Model = AiModel.GPT35Turbo,
                    MaxTokens = 10
                };

                var response = await SendMessageAsync(testRequest);
                return !string.IsNullOrEmpty(response.Answer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Configuration validation failed");
                return false;
            }
        }

        private async Task SaveConversationAsync(ChatRequest request, ChatResponse response)
        {
            try
            {
                var conversation = await _conversationRepository.GetByIdAsync(response.ConversationId)
                    ?? new Core.Models.Entities.Conversation
                    {
                        Id = response.ConversationId,
                        Title = GenerateConversationTitle(request.Message),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                // Add user message
                conversation.Messages.Add(new Core.Models.Entities.ConversationMessage
                {
                    Role = MessageRole.User,
                    Content = request.Message,
                    TokensUsed = 0, // You can calculate this from request
                    ModelUsed = OpenAIMapper.ToOpenAIModelString(request.Model),
                    Timestamp = DateTime.UtcNow
                });

                // Add assistant response
                conversation.Messages.Add(new Core.Models.Entities.ConversationMessage
                {
                    Role = MessageRole.Assistant,
                    Content = response.Answer,
                    TokensUsed = response.TokensUsed,
                    ModelUsed = response.ModelUsed,
                    Timestamp = response.Timestamp
                });

                conversation.UpdatedAt = DateTime.UtcNow;

                if (string.IsNullOrEmpty(conversation.Id))
                {
                    await _conversationRepository.AddAsync(conversation);
                }
                else
                {
                    await _conversationRepository.UpdateAsync(conversation);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save conversation to database");
                // Don't throw - we don't want to fail the main request if saving conversation fails
            }
        }

        private ConversationResponse MapToConversationResponse(Core.Models.Entities.Conversation conversation)
        {
            return new ConversationResponse
            {
                Id = conversation.Id,
                Title = conversation.Title,
                CreatedAt = conversation.CreatedAt,
                UpdatedAt = conversation.UpdatedAt,
                Messages = conversation.Messages.Select(m => new MessageRequest
                {
                    Role = m.Role,
                    Content = m.Content,
                    Timestamp = m.Timestamp
                }).ToList(),
                TotalMessages = conversation.Messages.Count
            };
        }

        private string GenerateConversationTitle(string firstMessage)
        {
            // Simple title generation - you can make this smarter
            if (firstMessage.Length > 50)
            {
                return firstMessage.Substring(0, 47) + "...";
            }
            return firstMessage;
        }

        private decimal CalculateEstimatedCost(int totalTokens)
        {
            // Rough cost estimation - adjust based on current OpenAI pricing
            const decimal costPerThousandTokens = 0.002m;
            return (totalTokens / 1000m) * costPerThousandTokens;
        }
    }
}