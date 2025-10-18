using IntelliBot.Core.Enums;
using IntelliBot.Core.Models.Requests;
using IntelliBot.Core.Models.Responses;
using IntelliBot.Infrastructure.Clients.Models;

namespace IntelliBot.Infrastructure.Clients
{
    public static class OpenAIMapper
    {
        public static string ToOpenAIModelString(AiModel model)
        {
            return model switch
            {
                AiModel.GPT35Turbo => "gpt-3.5-turbo",
                AiModel.GPT4 => "gpt-4",
                AiModel.GPT4Turbo => "gpt-4-turbo-preview",
                AiModel.GPT4o => "gpt-4o",

            // No changes to the existing code are required if the namespace IntelliBot.Core.Models.Responses contains the definition for ChatResponse.
                _ => "gpt-3.5-turbo"
            };
        }

        public static string ToOpenAIRoleString(MessageRole role)
        {
            return role switch
            {
                MessageRole.System => "system",
                MessageRole.User => "user",
                MessageRole.Assistant => "assistant",
                _ => "user"
            };
        }

        public static MessageRole ToMessageRole(string role)
        {
            return role?.ToLower() switch
            {
                "system" => MessageRole.System,
                "user" => MessageRole.User,
                "assistant" => MessageRole.Assistant,
                _ => MessageRole.User
            };
        }

        public static OpenAIRequest ToOpenAIRequest(ChatRequest request)
        {
            var openAIRequest = new OpenAIRequest
            {
                Model = ToOpenAIModelString(request.Model),
                MaxTokens = request.MaxTokens,
                Temperature = request.Temperature
            };

            // Add previous messages if any
            if (request.PreviousMessages?.Any() == true)
            {
                openAIRequest.Messages.AddRange(request.PreviousMessages.Select(m => new OpenAIMessage
                {
                    Role = ToOpenAIRoleString(m.Role),
                    Content = m.Content
                }));
            }

            // Add current message
            openAIRequest.Messages.Add(new OpenAIMessage
            {
                Role = "user",
                Content = request.Message
            });

            return openAIRequest;
        }

        public static ChatResponse ToChatResponse(OpenAIResponse openAIResponse, string conversationId, TimeSpan processingTime)
        {
            var response = new ChatResponse
            {
                ConversationId = conversationId,
                TokensUsed = GetTotalTokens(openAIResponse.Usage),
                ProcessingTime = processingTime,
                ModelUsed = openAIResponse.Model,
                Timestamp = DateTime.UtcNow
            };

            if (openAIResponse.Choices?.Count > 0)
            {
                // Cast the 'Message' object to the appropriate type (e.g., OpenAIMessage) to access the 'Content' property.
                if (openAIResponse.Choices[0].Message is OpenAIMessage message)
                {
                    response.Answer = message.Content;
                }
            }

            return response;
        }

        private static int GetTotalTokens(object usage)
        {
            // Assuming `usage` is a dynamic object or has a property `TotalTokens` at runtime.
            // If `usage` is a specific type, replace `dynamic` with the correct type.
            dynamic dynamicUsage = usage;
            return dynamicUsage.TotalTokens;
        }

        public static MessageRequest ToMessageRequest(OpenAIMessage message)
        {
            return new MessageRequest
            {
                Role = ToMessageRole(message.Role),
                Content = message.Content,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}