using IntelliBot.Core.Enums;
using IntelliBot.Core.Models;
using IntelliBot.Core.Models.Configuration;
using IntelliBot.Core.Models.Entities;
using IntelliBot.Core.Models.Requests;
using IntelliBot.Core.Models.Responses;
using Xunit;

namespace IntelliBot.Core.Tests
{
    public class AiModelTests
    {
        [Fact]
        public void AiModel_EnumValues_AreDefined()
        {
            // Arrange & Act & Assert
            Assert.Equal(0, (int)AiModel.GPT35Turbo);
            Assert.Equal(1, (int)AiModel.GPT4);
            Assert.Equal(2, (int)AiModel.GPT4Turbo);
            Assert.Equal(3, (int)AiModel.CustomModel);
            Assert.Equal(4, (int)AiModel.GPT4o);
        }
    }

    public class MessageRoleTests
    {
        [Fact]
        public void MessageRole_EnumValues_AreDefined()
        {
            // Arrange & Act & Assert
            Assert.Equal(1, (int)MessageRole.System);
            Assert.Equal(2, (int)MessageRole.User);
            Assert.Equal(3, (int)MessageRole.Assistant);
        }
    }

    public class AiModelInfoTests
    {
        [Fact]
        public void AiModelInfo_DefaultValues_AreSet()
        {
            // Arrange & Act
            var model = new AiModelInfo();

            // Assert
            Assert.Equal(string.Empty, model.Id);
            Assert.Equal(string.Empty, model.Name);
            Assert.Equal(string.Empty, model.Description);
            Assert.Equal(0, model.MaxTokens);
            Assert.False(model.SupportsVision);
            Assert.False(model.SupportsFunctionCalling);
            Assert.Equal(default(DateTime), model.ReleaseDate);
        }

        [Fact]
        public void AiModelInfo_Properties_CanBeSet()
        {
            // Arrange
            var releaseDate = new DateTime(2023, 3, 14);
            var model = new AiModelInfo
            {
                Id = "openai/gpt-4",
                Name = "GPT-4",
                Description = "Advanced AI model",
                MaxTokens = 8192,
                SupportsVision = false,
                SupportsFunctionCalling = true,
                ReleaseDate = releaseDate
            };

            // Assert
            Assert.Equal("openai/gpt-4", model.Id);
            Assert.Equal("GPT-4", model.Name);
            Assert.Equal("Advanced AI model", model.Description);
            Assert.Equal(8192, model.MaxTokens);
            Assert.False(model.SupportsVision);
            Assert.True(model.SupportsFunctionCalling);
            Assert.Equal(releaseDate, model.ReleaseDate);
        }
    }

    public class ChatRequestTests
    {
        [Fact]
        public void ChatRequest_DefaultValues_AreSet()
        {
            // Arrange & Act
            var request = new ChatRequest();

            // Assert
            Assert.Equal(string.Empty, request.Message);
        }

        [Fact]
        public void ChatRequest_Message_CanBeSet()
        {
            // Arrange
            var request = new ChatRequest { Message = "Hello, AI!" };

            // Assert
            Assert.Equal("Hello, AI!", request.Message);
        }
    }

    public class ChatSessionTests
    {
        [Fact]
        public void ChatSession_DefaultValues_AreSet()
        {
            // Arrange & Act
            var session = new ChatSession();

            // Assert
            Assert.Equal(string.Empty, session.Message);
            Assert.Null(session.ConversationId);
            Assert.Null(session.Model);
            Assert.Null(session.Temperature);
            Assert.Null(session.MaxTokens);
            Assert.Null(session.UserId);
            Assert.NotNull(session.PreviousMessages);
            Assert.Empty(session.PreviousMessages);
            Assert.Null(session.Title);
        }

        [Fact]
        public void ChatSession_Properties_CanBeSet()
        {
            // Arrange
            var session = new ChatSession
            {
                Message = "Test message",
                ConversationId = "conv-123",
                Model = "gpt-4",
                Temperature = 0.7,
                MaxTokens = 1000,
                UserId = "user-123",
                Title = "Test Conversation"
            };

            // Assert
            Assert.Equal("Test message", session.Message);
            Assert.Equal("conv-123", session.ConversationId);
            Assert.Equal("gpt-4", session.Model);
            Assert.Equal(0.7, session.Temperature);
            Assert.Equal(1000, session.MaxTokens);
            Assert.Equal("user-123", session.UserId);
            Assert.Equal("Test Conversation", session.Title);
        }
    }

    public class ChatResponseTests
    {
        [Fact]
        public void ChatResponse_DefaultValues_AreSet()
        {
            // Arrange & Act
            var response = new ChatResponse();

            // Assert
            Assert.Equal(string.Empty, response.Answer);
            Assert.Equal(string.Empty, response.ConversationId);
            Assert.Equal(0, response.TokensUsed);
            Assert.Equal(TimeSpan.Zero, response.ProcessingTime);
            Assert.Equal(string.Empty, response.ModelUsed);
            Assert.NotEqual(default(DateTime), response.Timestamp);
        }

        [Fact]
        public void ChatResponse_Properties_CanBeSet()
        {
            // Arrange
            var timestamp = DateTime.UtcNow;
            var processingTime = TimeSpan.FromSeconds(1.5);
            var response = new ChatResponse
            {
                Answer = "AI response",
                ConversationId = "conv-123",
                TokensUsed = 150,
                ProcessingTime = processingTime,
                ModelUsed = "gpt-4",
                Timestamp = timestamp
            };

            // Assert
            Assert.Equal("AI response", response.Answer);
            Assert.Equal("conv-123", response.ConversationId);
            Assert.Equal(150, response.TokensUsed);
            Assert.Equal(processingTime, response.ProcessingTime);
            Assert.Equal("gpt-4", response.ModelUsed);
            Assert.Equal(timestamp, response.Timestamp);
        }
    }

    public class ConversationResponseTests
    {
        [Fact]
        public void ConversationResponse_DefaultValues_AreSet()
        {
            // Arrange & Act
            var response = new ConversationResponse();

            // Assert
            Assert.Equal(string.Empty, response.Id);
            Assert.Equal(string.Empty, response.Title);
            Assert.Equal(default(DateTime), response.CreatedAt);
            Assert.Equal(default(DateTime), response.UpdatedAt);
            Assert.NotNull(response.Messages);
            Assert.Empty(response.Messages);
            Assert.Equal(0, response.TotalMessages);
            Assert.Equal(string.Empty, response.Answer);
        }

        [Fact]
        public void ConversationResponse_Properties_CanBeSet()
        {
            // Arrange
            var createdAt = DateTime.UtcNow.AddDays(-1);
            var updatedAt = DateTime.UtcNow;
            var messages = new List<MessageRequest>
            {
                new MessageRequest { Role = MessageRole.User, Content = "Hello" }
            };

            var response = new ConversationResponse
            {
                Id = "conv-123",
                Title = "Test Conversation",
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                Messages = messages,
                TotalMessages = 1,
                Answer = "Response"
            };

            // Assert
            Assert.Equal("conv-123", response.Id);
            Assert.Equal("Test Conversation", response.Title);
            Assert.Equal(createdAt, response.CreatedAt);
            Assert.Equal(updatedAt, response.UpdatedAt);
            Assert.Single(response.Messages);
            Assert.Equal(1, response.TotalMessages);
            Assert.Equal("Response", response.Answer);
        }
    }

    public class MessageRequestTests
    {
        [Fact]
        public void MessageRequest_DefaultValues_AreSet()
        {
            // Arrange & Act
            var message = new MessageRequest();

            // Assert
            Assert.Equal(MessageRole.System, message.Role);
            Assert.Equal(string.Empty, message.Content);
            Assert.NotEqual(default(DateTime), message.Timestamp);
        }

        [Fact]
        public void MessageRequest_Properties_CanBeSet()
        {
            // Arrange
            var timestamp = DateTime.UtcNow;
            var message = new MessageRequest
            {
                Role = MessageRole.User,
                Content = "Hello, AI!",
                Timestamp = timestamp
            };

            // Assert
            Assert.Equal(MessageRole.User, message.Role);
            Assert.Equal("Hello, AI!", message.Content);
            Assert.Equal(timestamp, message.Timestamp);
        }
    }

    public class UsageStatsResponseTests
    {
        [Fact]
        public void UsageStatsResponse_DefaultValues_AreSet()
        {
            // Arrange & Act
            var response = new UsageStatsResponse();

            // Assert
            Assert.Equal(default(DateTime), response.PeriodStart);
            Assert.Equal(default(DateTime), response.PeriodEnd);
            Assert.Equal(0, response.TotalRequests);
            Assert.Equal(0, response.TotalTokensUsed);
            Assert.Equal(0m, response.EstimatedCost);
            Assert.NotNull(response.RequestsPerModel);
            Assert.Empty(response.RequestsPerModel);
        }

        [Fact]
        public void UsageStatsResponse_Properties_CanBeSet()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var requestsPerModel = new Dictionary<string, int>
            {
                { "gpt-4", 10 },
                { "gpt-3.5-turbo", 5 }
            };

            var response = new UsageStatsResponse
            {
                PeriodStart = startDate,
                PeriodEnd = endDate,
                TotalRequests = 15,
                TotalTokensUsed = 1500,
                EstimatedCost = 0.0225m,
                RequestsPerModel = requestsPerModel
            };

            // Assert
            Assert.Equal(startDate, response.PeriodStart);
            Assert.Equal(endDate, response.PeriodEnd);
            Assert.Equal(15, response.TotalRequests);
            Assert.Equal(1500, response.TotalTokensUsed);
            Assert.Equal(0.0225m, response.EstimatedCost);
            Assert.Equal(2, response.RequestsPerModel.Count);
            Assert.Equal(10, response.RequestsPerModel["gpt-4"]);
            Assert.Equal(5, response.RequestsPerModel["gpt-3.5-turbo"]);
        }
    }

    public class ConversationTests
    {
        [Fact]
        public void Conversation_DefaultValues_AreSet()
        {
            // Arrange & Act
            var conversation = new Conversation();

            // Assert
            Assert.NotEqual(Guid.Empty, Guid.Parse(conversation.Id));
            Assert.Equal(string.Empty, conversation.Title);
            Assert.Null(conversation.UserId);
            Assert.NotEqual(default(DateTime), conversation.CreatedAt);
            Assert.NotEqual(default(DateTime), conversation.UpdatedAt);
            Assert.NotNull(conversation.Messages);
            Assert.Empty(conversation.Messages);
        }

        [Fact]
        public void Conversation_Properties_CanBeSet()
        {
            // Arrange
            var conversation = new Conversation
            {
                Id = "custom-id",
                Title = "Test Conversation",
                UserId = "user-123"
            };

            // Assert
            Assert.Equal("custom-id", conversation.Id);
            Assert.Equal("Test Conversation", conversation.Title);
            Assert.Equal("user-123", conversation.UserId);
        }

        [Fact]
        public void Conversation_UpdateTimestamp_UpdatesUpdatedAt()
        {
            // Arrange
            var conversation = new Conversation();
            var originalUpdatedAt = conversation.UpdatedAt;

            // Act
            Thread.Sleep(1); // Ensure time difference
            conversation.UpdateTimestamp();

            // Assert
            Assert.True(conversation.UpdatedAt > originalUpdatedAt);
        }
    }

    public class ConversationMessageTests
    {
        [Fact]
        public void ConversationMessage_DefaultValues_AreSet()
        {
            // Arrange & Act
            var message = new ConversationMessage();

            // Assert
            Assert.NotEqual(Guid.Empty, Guid.Parse(message.Id));
            Assert.Equal(string.Empty, message.ConversationId);
            Assert.Equal(MessageRole.System, message.Role);
            Assert.Equal(string.Empty, message.Content);
            Assert.Equal(0, message.TokensUsed);
            Assert.NotEqual(default(DateTime), message.Timestamp);
            Assert.Equal(string.Empty, message.ModelUsed);
        }

        [Fact]
        public void ConversationMessage_Properties_CanBeSet()
        {
            // Arrange
            var timestamp = DateTime.UtcNow;
            var message = new ConversationMessage
            {
                Id = "msg-123",
                ConversationId = "conv-123",
                Role = MessageRole.User,
                Content = "Hello, AI!",
                TokensUsed = 50,
                Timestamp = timestamp,
                ModelUsed = "gpt-4"
            };

            // Assert
            Assert.Equal("msg-123", message.Id);
            Assert.Equal("conv-123", message.ConversationId);
            Assert.Equal(MessageRole.User, message.Role);
            Assert.Equal("Hello, AI!", message.Content);
            Assert.Equal(50, message.TokensUsed);
            Assert.Equal(timestamp, message.Timestamp);
            Assert.Equal("gpt-4", message.ModelUsed);
        }
    }

    public class OpenAIConfigTests
    {
        [Fact]
        public void OpenAIConfig_DefaultValues_AreSet()
        {
            // Arrange & Act
            var config = new OpenAIConfig();

            // Assert
            Assert.Equal(string.Empty, config.ApiKey);
            Assert.Null(config.Organization);
            Assert.Equal("https://openrouter.ai/api/v1", config.BaseUrl);
            Assert.Equal("openai/gpt-4o", config.DefaultModel);
            Assert.Equal(1000, config.MaxTokens);
            Assert.Equal(0.7, config.Temperature);
            Assert.Equal(TimeSpan.FromSeconds(30), config.Timeout);
        }

        [Fact]
        public void OpenAIConfig_Properties_CanBeSet()
        {
            // Arrange
            var config = new OpenAIConfig
            {
                ApiKey = "test-key",
                Organization = "test-org",
                BaseUrl = "https://custom.api.com",
                DefaultModel = "gpt-4",
                MaxTokens = 2000,
                Temperature = 0.5,
                Timeout = TimeSpan.FromMinutes(1)
            };

            // Assert
            Assert.Equal("test-key", config.ApiKey);
            Assert.Equal("test-org", config.Organization);
            Assert.Equal("https://custom.api.com", config.BaseUrl);
            Assert.Equal("gpt-4", config.DefaultModel);
            Assert.Equal(2000, config.MaxTokens);
            Assert.Equal(0.5, config.Temperature);
            Assert.Equal(TimeSpan.FromMinutes(1), config.Timeout);
        }
    }

    public class CacheConfigTests
    {
        [Fact]
        public void CacheConfig_DefaultValues_AreSet()
        {
            // Arrange & Act
            var config = new CacheConfig();

            // Assert
            Assert.True(config.Enabled);
            Assert.Equal(30, config.TimeoutInMinutes);
        }

        [Fact]
        public void CacheConfig_Properties_CanBeSet()
        {
            // Arrange
            var config = new CacheConfig
            {
                Enabled = false,
                TimeoutInMinutes = 60
            };

            // Assert
            Assert.False(config.Enabled);
            Assert.Equal(60, config.TimeoutInMinutes);
        }
    }
}
