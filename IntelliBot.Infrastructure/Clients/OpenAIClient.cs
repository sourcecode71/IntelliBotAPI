using System.Text;
using System.Text.Json;
using IntelliBot.Core.Interfaces;
using IntelliBot.Core.Models.Configuration;
using IntelliBot.Infrastructure.Clients.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options; // Add this namespace for ILogger<>



namespace IntelliBot.Infrastructure.Clients
{
    public class OpenAIClient : IOpenAIClient
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAIConfig _config;
        private readonly ILogger<OpenAIClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public OpenAIClient(
            HttpClient httpClient,
            IOptions<OpenAIConfig> config,
            ILogger<OpenAIClient> logger)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            // Set up HttpClient
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");

            if (!string.IsNullOrEmpty(_config.Organization))
            {
                _httpClient.DefaultRequestHeaders.Add("OpenAI-Organization", _config.Organization);
            }

            _httpClient.Timeout = _config.Timeout;
        }

        public async Task<OpenAIResponse> GetChatCompletionAsync(OpenAIRequest request)
        {
            try
            {
                _logger.LogDebug("Sending OpenAI chat completion request for model {Model}", request.Model);

                var jsonContent = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent, _jsonOptions);

                if (openAIResponse == null)
                {
                    throw new InvalidOperationException("Failed to deserialize OpenAI response");
                }

                _logger.LogDebug(
                    "OpenAI response received: {Tokens} tokens used, {Choices} choices",
                    openAIResponse.Usage.TotalTokens, openAIResponse.Choices.Count);

                return openAIResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while calling OpenAI API");
                throw new OpenAIApiException("Error calling OpenAI API", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while calling OpenAI API");
                throw new OpenAIApiException("Unexpected error calling OpenAI API", ex);
            }
        }

        public async Task StreamChatCompletionAsync(OpenAIRequest request, Func<string, Task> onTokenReceived, CancellationToken cancellationToken = default)
        {
            try
            {
                request.Stream = true;

                var jsonContent = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("chat/completions", content, cancellationToken);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var reader = new StreamReader(stream);

                while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(cancellationToken);
                    if (string.IsNullOrEmpty(line) || !line.StartsWith("data: "))
                        continue;

                    var data = line["data: ".Length..];
                    if (data == "[DONE]")
                        break;

                    try
                    {
                        var streamResponse = JsonSerializer.Deserialize<OpenAIStreamResponse>(data, _jsonOptions);
                        var token = streamResponse?.Choices?.FirstOrDefault()?.Delta?.Content;

                        if (!string.IsNullOrEmpty(token))
                        {
                            await onTokenReceived(token);
                        }
                    }
                    catch (JsonException)
                    {
                        // Ignore JSON errors in streaming
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OpenAI streaming");
                throw new OpenAIApiException("Error during streaming", ex);
            }
        }

        public async Task<bool> ValidateApiKeyAsync()
        {
            try
            {
                // Use the models endpoint to validate the API key
                var response = await _httpClient.GetAsync("models");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate OpenAI API key");
                return false;
            }
        }
    }

    public class OpenAIApiException : Exception
    {
        public OpenAIApiException(string message) : base(message) { }
        public OpenAIApiException(string message, Exception innerException) : base(message, innerException) { }
    }

    // Stream response models
    public class OpenAIStreamResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Object { get; set; } = string.Empty;
        public long Created { get; set; }
        public string Model { get; set; } = string.Empty;
        public List<OpenAIStreamChoice> Choices { get; set; } = new List<OpenAIStreamChoice>();
    }

    public class OpenAIStreamChoice
    {
        public OpenAIStreamDelta Delta { get; set; } = new OpenAIStreamDelta();
        public int Index { get; set; }
        public string FinishReason { get; set; } = string.Empty;
    }

    public class OpenAIStreamDelta
    {
        public string Content { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
    // Fix for CS1061: 'object' does not contain a definition for 'TotalTokens'
    // The issue occurs because the `Usage` property in `OpenAIResponse` is of type `object`.
    // To fix this, we need to define a proper type for `Usage` and update the `OpenAIResponse` class.

    public class OpenAIUsage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }

    public class OpenAIResponse
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public long Created { get; set; }
        public string Model { get; set; }
        public List<OpenAIChoice> Choices { get; set; }
        public OpenAIUsage Usage { get; set; } // Updated from 'object' to 'OpenAIUsage'
    }
}