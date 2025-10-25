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
                AiModel.GPT35Turbo => "openai/gpt-3.5-turbo",
                AiModel.GPT4 => "openai/gpt-4",
                AiModel.GPT4Turbo => "openai/gpt-4-turbo",
                AiModel.GPT4o => "openai/gpt-4o",
                _ => "openai/gpt-3.5-turbo"
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

        public static OpenAIRequest ToOpenAIRequest(ChatSession session)
        {
            var openAIRequest = new OpenAIRequest
            {
                Model = session.Model ?? "openai/gpt-4o", // Fallback to default
                MaxTokens = session.MaxTokens ?? 1000,    // Fallback to default
                Temperature = session.Temperature ?? 0.7  // Fallback to default
            };

            // Add previous messages from database
            if (session.PreviousMessages?.Any() == true)
            {
                openAIRequest.Messages.AddRange(session.PreviousMessages.Select(m => new OpenAIMessage
                {
                    Role = ToOpenAIRoleString(m.Role),
                    Content = m.Content
                }));
            }

            // Add current message from user
            openAIRequest.Messages.Add(new OpenAIMessage
            {
                Role = "user",
                Content = session.Message
            });

            return openAIRequest;
        }

        public static ChatResponse ToChatResponse(OpenAIResponse openAIResponse, string conversationId, TimeSpan processingTime)
        {
            var response = new ChatResponse
            {
                ConversationId = conversationId,
                TokensUsed = openAIResponse.Usage?.TotalTokens ?? 0,
                ProcessingTime = processingTime,
                ModelUsed = openAIResponse.Model,
                Timestamp = DateTime.UtcNow
            };

            if (openAIResponse.Choices?.Count > 0 && openAIResponse.Choices[0].Message is OpenAIMessage message)
            {
                response.Answer = message.Content;
            }

            return response;
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