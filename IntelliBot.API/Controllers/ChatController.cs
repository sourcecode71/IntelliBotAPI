using IntelliBot.Core.Interfaces;
using IntelliBot.Core.Models;
using IntelliBot.Core.Models.Requests;
using IntelliBot.Core.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace IntelliBot.API.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        // <summary>
        /// Send a message to the AI assistant
        /// </summary>
        /// <param name="request">Chat request with message and configuration</param>
        /// <returns>AI assistant response</returns>
        [HttpPost("message")]
        [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ChatResponse>> SendMessage([FromBody] ChatRequest request)
        {
            try
            {
                _logger.LogInformation("Processing chat message: {Message}", request.Message);

                var response = await _chatService.SendMessageAsync(request);

                _logger.LogInformation("Successfully processed chat message with {Tokens} tokens", response.TokensUsed);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat message");
                return Problem(
                    title: "Error processing request",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Send a message with streaming response for real-time updates
        /// </summary>
        /// <param name="request">Chat request with message and configuration</param>
        /// <returns>Stream of text chunks</returns>
        [HttpPost("message/stream")]
        [Produces("text/plain")]
        public async Task SendMessageStream([FromBody] ChatRequest request)
        {
            Response.ContentType = "text/plain";
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            try
            {
                await _chatService.StreamMessageAsync(
                    request,
                    async (token) =>
                    {
                        await Response.WriteAsync(token);
                        await Response.Body.FlushAsync();
                    },
                    HttpContext.RequestAborted);

                await Response.CompleteAsync();
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Streaming request was cancelled by client");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during streaming chat message");
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                await Response.WriteAsync($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a specific conversation by ID
        /// </summary>
        /// <param name="conversationId">The conversation ID</param>
        /// <returns>Conversation with all messages</returns>
        [HttpGet("conversations/{conversationId}")]
        [ProducesResponseType(typeof(ConversationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ConversationResponse>> GetConversation(
            [Required] string conversationId)
        {
            try
            {
                var conversation = await _chatService.GetConversationAsync(conversationId);
                return Ok(conversation);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Conversation not found",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversation {ConversationId}", conversationId);
                return Problem(
                    title: "Error retrieving conversation",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get all conversations with pagination
        /// </summary>
        /// <param name="userId">Optional user ID filter</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 20, max: 100)</param>
        /// <returns>List of conversations</returns>
        [HttpGet("conversations")]
        [ProducesResponseType(typeof(List<ConversationResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ConversationResponse>>> GetConversations(
            [FromQuery] string? userId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                // Validate pagination parameters
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 1;
                if (pageSize > 100) pageSize = 100;

                var conversations = await _chatService.GetConversationsAsync(userId, page, pageSize);
                return Ok(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversations for user {UserId}", userId);
                return Problem(
                    title: "Error retrieving conversations",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        ///// <summary>
        ///// Create a new conversation
        ///// </summary>
        ///// <param name="request">Conversation creation request</param>
        ///// <returns>Created conversation</returns>
        //[HttpPost("conversations")]
        //[ProducesResponseType(typeof(ConversationResponse), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        //public async Task<ActionResult<ConversationResponse>> CreateConversation(
        //    [FromBody] ChatRequest request)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(request.Title))
        //        {
        //            return BadRequest(new ProblemDetails
        //            {
        //                Title = "Invalid request",
        //                Detail = "Title is required",
        //                Status = StatusCodes.Status400BadRequest
        //            });
        //        }

        //        var conversation = await _chatService.CreateConversationAsync(request);
        //        return Ok(conversation);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error creating conversation with title {Title}", request.Title);
        //        return Problem(
        //            title: "Error creating conversation",
        //            detail: ex.Message,
        //            statusCode: StatusCodes.Status500InternalServerError);
        //    }
        //}

        /// <summary>
        /// Delete a conversation
        /// </summary>
        /// <param name="conversationId">The conversation ID to delete</param>
        /// <returns>Success status</returns>
        [HttpDelete("conversations/{conversationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteConversation(
            [Required] string conversationId)
        {
            try
            {
                var result = await _chatService.DeleteConversationAsync(conversationId);

                if (result)
                {
                    return Ok(new { message = "Conversation deleted successfully" });
                }
                else
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Conversation not found",
                        Detail = $"Conversation with ID {conversationId} was not found",
                        Status = StatusCodes.Status404NotFound
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting conversation {ConversationId}", conversationId);
                return Problem(
                    title: "Error deleting conversation",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Update conversation title
        /// </summary>
        /// <param name="conversationId">The conversation ID</param>
        /// <param name="title">New title for the conversation</param>
        /// <returns>Updated conversation</returns>
        [HttpPatch("conversations/{conversationId}/title")]
        [ProducesResponseType(typeof(ConversationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ConversationResponse>> UpdateConversationTitle(
            [Required] string conversationId,
            [FromBody] UpdateTitleRequest titleRequest)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(titleRequest.Title))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid request",
                        Detail = "Title is required",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var conversation = await _chatService.UpdateConversationTitleAsync(conversationId, titleRequest.Title);
                return Ok(conversation);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Conversation not found",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating conversation title for {ConversationId}", conversationId);
                return Problem(
                    title: "Error updating conversation",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get usage statistics for a period
        /// </summary>
        /// <param name="fromDate">Start date (ISO 8601 format)</param>
        /// <param name="toDate">End date (ISO 8601 format)</param>
        /// <param name="userId">Optional user ID filter</param>
        /// <returns>Usage statistics</returns>
        [HttpGet("usage")]
        [ProducesResponseType(typeof(UsageStatsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UsageStatsResponse>> GetUsageStatistics(
            [FromQuery][Required] DateTime fromDate,
            [FromQuery][Required] DateTime toDate,
            [FromQuery] string? userId = null)
        {
            try
            {
                // Validate date range
                if (fromDate > toDate)
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid date range",
                        Detail = "From date cannot be after to date",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                // Limit date range to prevent excessive data processing
                if ((toDate - fromDate).TotalDays > 365)
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Date range too large",
                        Detail = "Date range cannot exceed 365 days",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var stats = await _chatService.GetUsageStatisticsAsync(fromDate, toDate, userId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving usage statistics from {FromDate} to {ToDate}", fromDate, toDate);
                return Problem(
                    title: "Error retrieving usage statistics",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get available AI models
        /// </summary>
        /// <returns>List of available models with capabilities</returns>
        [HttpGet("models")]
        [ProducesResponseType(typeof(List<AiModelInfo>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AiModelInfo>>> GetAvailableModels()
        {
            try
            {
                var models = await _chatService.GetAvailableModelsAsync();
                return Ok(models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available models");
                return Problem(
                    title: "Error retrieving models",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Validate service configuration
        /// </summary>
        /// <returns>Validation result</returns>
        [HttpGet("validate")]
        [ProducesResponseType(typeof(ValidationResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ValidationResponse>> ValidateConfiguration()
        {
            try
            {
                var isValid = await _chatService.ValidateConfigurationAsync();

                return Ok(new ValidationResponse
                {
                    IsValid = isValid,
                    Timestamp = DateTime.UtcNow,
                    Message = isValid ? "Service is properly configured" : "Service configuration is invalid"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating service configuration");
                return Ok(new ValidationResponse
                {
                    IsValid = false,
                    Timestamp = DateTime.UtcNow,
                    Message = $"Validation failed: {ex.Message}"
                });
            }
        }
    }

    // Supporting request models for the controller
    public class UpdateTitleRequest
    {
        public string Title { get; set; } = string.Empty;
    }

    public class ValidationResponse
    {
        public bool IsValid { get; set; }
        public DateTime Timestamp { get; set; }
        public string Message { get; set; } = string.Empty;
    }


}
