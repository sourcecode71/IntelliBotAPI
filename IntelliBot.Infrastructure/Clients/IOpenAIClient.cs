using IntelliBot.Infrastructure.Clients.Models;

namespace IntelliBot.Infrastructure.Clients
{
    public interface IOpenAIClient
    {
        /// <summary>
        /// Sends a chat completion request to OpenAI API
        /// </summary>
        /// <param name="request">The chat completion request</param>
        /// <returns>OpenAI chat completion response</returns>
        Task<OpenAIResponse> GetChatCompletionAsync(OpenAIRequest request);

        /// <summary>
        /// Streams chat completion response from OpenAI API
        /// </summary>
        /// <param name="request">The chat completion request</param>
        /// <param name="onTokenReceived">Callback for each token received</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the streaming operation</returns>
        Task StreamChatCompletionAsync(OpenAIRequest request, Func<string, Task> onTokenReceived, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates the OpenAI API key and configuration
        /// </summary>
        /// <returns>True if the API is accessible</returns>
        Task<bool> ValidateApiKeyAsync();
    }
}