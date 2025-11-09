using IntelliBot.API.Controllers;
using IntelliBot.Core.Interfaces;
using IntelliBot.Core.Models.Requests;
using IntelliBot.Core.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace IntelliBot.API.Tests
{
    public class ChatControllerTests
    {
        private readonly Mock<IChatService> _chatServiceMock;
        private readonly Mock<ILogger<ChatController>> _loggerMock;
        private readonly ChatController _controller;

        public ChatControllerTests()
        {
            _chatServiceMock = new Mock<IChatService>();
            _loggerMock = new Mock<ILogger<ChatController>>();
            _controller = new ChatController(_chatServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task SendMessage_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var request = new ChatRequest { Message = "Hello, AI!" };
            var response = new ChatResponse
            {
                Answer = "Hello, human!",
                ConversationId = "conv-123",
                TokensUsed = 50,
                ModelUsed = "gpt-4",
                Timestamp = DateTime.UtcNow
            };

            _chatServiceMock.Setup(s => s.SendMessageAsync(request)).ReturnsAsync(response);

            // Act
            var result = await _controller.SendMessage(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedResponse = Assert.IsType<ChatResponse>(okResult.Value);
            Assert.Equal("Hello, human!", returnedResponse.Answer);
            Assert.Equal("conv-123", returnedResponse.ConversationId);
        }

        [Fact]
        public async Task SendMessage_WithException_ReturnsProblemResult()
        {
            // Arrange
            var request = new ChatRequest { Message = "Hello, AI!" };
            var exception = new Exception("Service error");

            _chatServiceMock.Setup(s => s.SendMessageAsync(request)).ThrowsAsync(exception);

            // Act
            var result = await _controller.SendMessage(request);

            // Assert
            var problemResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, problemResult.StatusCode);
            var problemDetails = Assert.IsType<ProblemDetails>(problemResult.Value);
            Assert.Contains("Service error", problemDetails.Detail);
        }

        [Fact]
        public async Task SendMessageStream_WithValidRequest_StreamsResponse()
        {
            // Arrange
            var request = new ChatRequest { Message = "Stream this message" };
            var httpContext = new DefaultHttpContext();
            var responseBody = new MemoryStream();
            httpContext.Response.Body = responseBody;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            _chatServiceMock.Setup(s => s.StreamMessageAsync(
                request,
                It.IsAny<Func<string, Task>>(),
                It.IsAny<CancellationToken>()))
                .Callback<ChatRequest, Func<string, Task>, CancellationToken>(async (req, onToken, ct) =>
                {
                    await onToken("Hello");
                    await onToken(" ");
                    await onToken("world");
                })
                .Returns(Task.CompletedTask);

            // Act
            await _controller.SendMessageStream(request);

            // Assert
            Assert.Equal("text/plain", httpContext.Response.ContentType);
            Assert.Equal("no-cache", httpContext.Response.Headers.CacheControl);
            Assert.Equal("keep-alive", httpContext.Response.Headers.Connection);

            responseBody.Position = 0;
            var reader = new StreamReader(responseBody);
            var content = await reader.ReadToEndAsync();
            Assert.Equal("Hello world", content);
        }

        [Fact]
        public async Task SendMessageStream_WithException_WritesErrorToResponse()
        {
            // Arrange
            var request = new ChatRequest { Message = "Stream this message" };
            var httpContext = new DefaultHttpContext();
            var responseBody = new MemoryStream();
            httpContext.Response.Body = responseBody;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            _chatServiceMock.Setup(s => s.StreamMessageAsync(
                request,
                It.IsAny<Func<string, Task>>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Streaming error"));

            // Act
            await _controller.SendMessageStream(request);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);

            responseBody.Position = 0;
            var reader = new StreamReader(responseBody);
            var content = await reader.ReadToEndAsync();
            Assert.Contains("Streaming error", content);
        }

        [Fact]
        public async Task SendMessageStream_WithCancellationException_LogsInformation()
        {
            // Arrange
            var request = new ChatRequest { Message = "Stream this message" };
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            _chatServiceMock.Setup(s => s.StreamMessageAsync(
                request,
                It.IsAny<Func<string, Task>>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            // Act
            await _controller.SendMessageStream(request);

            // Assert - OperationCanceledException should be handled gracefully
            // We can't easily test logging, but we can verify no exception is thrown
        }

        [Fact]
        public async Task DeleteConversation_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var conversationId = "conv-123";
            _chatServiceMock.Setup(s => s.DeleteConversationAsync(conversationId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteConversation(conversationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.Equal("Conversation deleted successfully", response.message);
        }

        [Fact]
        public async Task DeleteConversation_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            var conversationId = "invalid-conv";
            _chatServiceMock.Setup(s => s.DeleteConversationAsync(conversationId)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteConversation(conversationId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var problemDetails = Assert.IsType<ProblemDetails>(notFoundResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
            Assert.Contains($"Conversation with ID {conversationId} was not found", problemDetails.Detail);
        }

        [Fact]
        public async Task DeleteConversation_WithException_ReturnsProblemResult()
        {
            // Arrange
            var conversationId = "conv-123";
            var exception = new Exception("Database error");

            _chatServiceMock.Setup(s => s.DeleteConversationAsync(conversationId)).ThrowsAsync(exception);

            // Act
            var result = await _controller.DeleteConversation(conversationId);

            // Assert
            var problemResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, problemResult.StatusCode);
            var problemDetails = Assert.IsType<ProblemDetails>(problemResult.Value);
            Assert.Contains("Database error", problemDetails.Detail);
        }

        [Fact]
        public async Task UpdateConversationTitle_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var conversationId = "conv-123";
            var titleRequest = new ChatController.UpdateTitleRequest { Title = "New Title" };
            var updatedConversation = new ConversationResponse
            {
                Id = conversationId,
                Title = "New Title",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            };

            _chatServiceMock.Setup(s => s.UpdateConversationTitleAsync(conversationId, "New Title"))
                .ReturnsAsync(updatedConversation);

            // Act
            var result = await _controller.UpdateConversationTitle(conversationId, titleRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedConversation = Assert.IsType<ConversationResponse>(okResult.Value);
            Assert.Equal("New Title", returnedConversation.Title);
        }

        [Fact]
        public async Task UpdateConversationTitle_WithEmptyTitle_ReturnsBadRequest()
        {
            // Arrange
            var conversationId = "conv-123";
            var titleRequest = new ChatController.UpdateTitleRequest { Title = "" };

            // Act
            var result = await _controller.UpdateConversationTitle(conversationId, titleRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
            Assert.Equal("Title is required", problemDetails.Detail);
        }

        [Fact]
        public async Task UpdateConversationTitle_WithWhitespaceTitle_ReturnsBadRequest()
        {
            // Arrange
            var conversationId = "conv-123";
            var titleRequest = new ChatController.UpdateTitleRequest { Title = "   " };

            // Act
            var result = await _controller.UpdateConversationTitle(conversationId, titleRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
            Assert.Equal("Title is required", problemDetails.Detail);
        }

        [Fact]
        public async Task UpdateConversationTitle_WithInvalidConversationId_ReturnsNotFound()
        {
            // Arrange
            var conversationId = "invalid-conv";
            var titleRequest = new ChatController.UpdateTitleRequest { Title = "New Title" };

            _chatServiceMock.Setup(s => s.UpdateConversationTitleAsync(conversationId, "New Title"))
                .ThrowsAsync(new ArgumentException("Conversation not found"));

            // Act
            var result = await _controller.UpdateConversationTitle(conversationId, titleRequest);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var problemDetails = Assert.IsType<ProblemDetails>(notFoundResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
            Assert.Contains("Conversation not found", problemDetails.Detail);
        }

        [Fact]
        public async Task GetUsageStatistics_WithValidDates_ReturnsOkResult()
        {
            // Arrange
            var fromDate = DateTime.UtcNow.AddDays(-7);
            var toDate = DateTime.UtcNow;
            var usageStats = new UsageStatsResponse
            {
                PeriodStart = fromDate,
                PeriodEnd = toDate,
                TotalRequests = 100,
                TotalTokensUsed = 5000,
                EstimatedCost = 0.075m
            };

            _chatServiceMock.Setup(s => s.GetUsageStatisticsAsync(fromDate, toDate, null)).ReturnsAsync(usageStats);

            // Act
            var result = await _controller.GetUsageStatistics(fromDate, toDate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedStats = Assert.IsType<UsageStatsResponse>(okResult.Value);
            Assert.Equal(100, returnedStats.TotalRequests);
            Assert.Equal(5000, returnedStats.TotalTokensUsed);
        }

        [Fact]
        public async Task GetUsageStatistics_WithFromDateAfterToDate_ReturnsBadRequest()
        {
            // Arrange
            var fromDate = DateTime.UtcNow;
            var toDate = DateTime.UtcNow.AddDays(-1);

            // Act
            var result = await _controller.GetUsageStatistics(fromDate, toDate);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
            Assert.Equal("From date cannot be after to date", problemDetails.Detail);
        }

        [Fact]
        public async Task GetUsageStatistics_WithDateRangeTooLarge_ReturnsBadRequest()
        {
            // Arrange
            var fromDate = DateTime.UtcNow.AddDays(-400);
            var toDate = DateTime.UtcNow;

            // Act
            var result = await _controller.GetUsageStatistics(fromDate, toDate);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
            Assert.Equal("Date range cannot exceed 365 days", problemDetails.Detail);
        }

        [Fact]
        public async Task GetAvailableModels_ReturnsOkResult()
        {
            // Arrange
            var models = new List<AiModelInfo>
            {
                new AiModelInfo { Id = "gpt-4", Name = "GPT-4", MaxTokens = 8192 },
                new AiModelInfo { Id = "gpt-3.5-turbo", Name = "GPT-3.5 Turbo", MaxTokens = 4096 }
            };

            _chatServiceMock.Setup(s => s.GetAvailableModelsAsync()).ReturnsAsync(models);

            // Act
            var result = await _controller.GetAvailableModels();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedModels = Assert.IsType<List<AiModelInfo>>(okResult.Value);
            Assert.Equal(2, returnedModels.Count);
            Assert.Equal("gpt-4", returnedModels[0].Id);
        }

        [Fact]
        public async Task ValidateConfiguration_ReturnsOkResult()
        {
            // Arrange
            _chatServiceMock.Setup(s => s.ValidateConfigurationAsync()).ReturnsAsync(true);

            // Act
            var result = await _controller.ValidateConfiguration();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var validationResponse = Assert.IsType<ChatController.ValidationResponse>(okResult.Value);
            Assert.True(validationResponse.IsValid);
            Assert.Equal("Service is properly configured", validationResponse.Message);
        }

        [Fact]
        public async Task ValidateConfiguration_WithInvalidConfig_ReturnsOkWithFalseResult()
        {
            // Arrange
            _chatServiceMock.Setup(s => s.ValidateConfigurationAsync()).ReturnsAsync(false);

            // Act
            var result = await _controller.ValidateConfiguration();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var validationResponse = Assert.IsType<ChatController.ValidationResponse>(okResult.Value);
            Assert.False(validationResponse.IsValid);
            Assert.Equal("Service configuration is invalid", validationResponse.Message);
        }

        [Fact]
        public async Task ValidateConfiguration_WithException_ReturnsOkWithErrorMessage()
        {
            // Arrange
            var exception = new Exception("API key invalid");
            _chatServiceMock.Setup(s => s.ValidateConfigurationAsync()).ThrowsAsync(exception);

            // Act
            var result = await _controller.ValidateConfiguration();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var validationResponse = Assert.IsType<ChatController.ValidationResponse>(okResult.Value);
            Assert.False(validationResponse.IsValid);
            Assert.Contains("API key invalid", validationResponse.Message);
        }
    }
}