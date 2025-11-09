using IntelliBot.Core.Interfaces;
using IntelliBot.Core.Models.Entities;
using IntelliBot.Core.Enums;
using IntelliBot.Infrastructure.Repositories;
using IntelliBot.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IntelliBot.Infrastructure.Tests
{
    public class ConversationRepositoryTests
    {
        private readonly ConversationRepository _repository;
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly Mock<ILogger<ConversationRepository>> _loggerMock;

        public ConversationRepositoryTests()
        {
            _memoryCacheMock = new Mock<IMemoryCache>();
            _loggerMock = new Mock<ILogger<ConversationRepository>>();
            _repository = new ConversationRepository(_memoryCacheMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsConversation()
        {
            // Arrange
            var conversation = new Conversation
            {
                Id = "test-conv",
                Title = "Test Conversation",
                UserId = "test-user"
            };

            // Mock cache miss first, then hit
            object? cachedValue = null;
            _memoryCacheMock.Setup(c => c.TryGetValue($"conversation_{conversation.Id}", out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(c => c.Set(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<MemoryCacheEntryOptions>()));

            // Act
            await _repository.AddAsync(conversation);
            var result = await _repository.GetByIdAsync(conversation.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(conversation.Id, result.Id);
            Assert.Equal(conversation.Title, result.Title);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var invalidId = "non-existent";

            object? cachedValue = null;
            _memoryCacheMock.Setup(c => c.TryGetValue($"conversation_{invalidId}", out cachedValue))
                .Returns(false);

            // Act
            var result = await _repository.GetByIdAsync(invalidId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_WithCachedConversation_ReturnsFromCache()
        {
            // Arrange
            var conversation = new Conversation { Id = "cached-conv", Title = "Cached" };
            object? cachedValue = conversation;
            _memoryCacheMock.Setup(c => c.TryGetValue($"conversation_{conversation.Id}", out cachedValue))
                .Returns(true);

            // Act
            var result = await _repository.GetByIdAsync(conversation.Id);

            // Assert
            Assert.Equal(conversation, result);
        }

        [Fact]
        public async Task AddAsync_WithValidConversation_AddsToStorage()
        {
            // Arrange
            var conversation = new Conversation
            {
                Id = "new-conv",
                Title = "New Conversation",
                UserId = "test-user"
            };

            _memoryCacheMock.Setup(c => c.Set(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<MemoryCacheEntryOptions>()));

            // Act
            await _repository.AddAsync(conversation);

            // Assert
            var result = await _repository.GetByIdAsync(conversation.Id);
            Assert.NotNull(result);
            Assert.Equal(conversation.Id, result.Id);
        }

        [Fact]
        public async Task AddAsync_WithDuplicateId_ThrowsInvalidOperationException()
        {
            // Arrange
            var conversation1 = new Conversation { Id = "duplicate", Title = "First" };
            var conversation2 = new Conversation { Id = "duplicate", Title = "Second" };

            _memoryCacheMock.Setup(c => c.Set(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<MemoryCacheEntryOptions>()));

            // Act & Assert
            await _repository.AddAsync(conversation1);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.AddAsync(conversation2));
        }

        [Fact]
        public async Task UpdateAsync_WithValidConversation_UpdatesStorage()
        {
            // Arrange
            var conversation = new Conversation
            {
                Id = "update-conv",
                Title = "Original Title",
                UserId = "test-user"
            };

            _memoryCacheMock.Setup(c => c.Set(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<MemoryCacheEntryOptions>()));

            await _repository.AddAsync(conversation);

            // Update
            conversation.Title = "Updated Title";
            conversation.UpdateTimestamp();

            // Act
            await _repository.UpdateAsync(conversation);

            // Assert
            var result = await _repository.GetByIdAsync(conversation.Id);
            Assert.NotNull(result);
            Assert.Equal("Updated Title", result.Title);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentConversation_ThrowsArgumentException()
        {
            // Arrange
            var nonExistentConversation = new Conversation { Id = "non-existent", Title = "Test" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.UpdateAsync(nonExistentConversation));
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_RemovesFromStorage()
        {
            // Arrange
            var conversation = new Conversation { Id = "delete-conv", Title = "To Delete" };
            _memoryCacheMock.Setup(c => c.Set(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<MemoryCacheEntryOptions>()));
            _memoryCacheMock.Setup(c => c.Remove(It.IsAny<object>()));

            await _repository.AddAsync(conversation);

            // Act
            var result = await _repository.DeleteAsync(conversation.Id);

            // Assert
            Assert.True(result);
            var deleted = await _repository.GetByIdAsync(conversation.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
        {
            // Arrange
            var invalidId = "non-existent";

            // Act
            var result = await _repository.DeleteAsync(invalidId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetAllAsync_WithUserId_ReturnsFilteredConversations()
        {
            // Arrange
            var user1 = "user1";
            var user2 = "user2";

            var conv1 = new Conversation { Id = "conv1", UserId = user1, Title = "User1 Conv1" };
            var conv2 = new Conversation { Id = "conv2", UserId = user1, Title = "User1 Conv2" };
            var conv3 = new Conversation { Id = "conv3", UserId = user2, Title = "User2 Conv1" };

            _memoryCacheMock.Setup(c => c.Set(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<MemoryCacheEntryOptions>()));

            await _repository.AddAsync(conv1);
            await _repository.AddAsync(conv2);
            await _repository.AddAsync(conv3);

            // Act
            var result = await _repository.GetAllAsync(user1);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, c => Assert.Equal(user1, c.UserId));
        }

        [Fact]
        public async Task GetAllAsync_WithPagination_ReturnsPagedResults()
        {
            // Arrange
            for (int i = 1; i <= 10; i++)
            {
                var conv = new Conversation { Id = $"conv{i}", UserId = "user", Title = $"Conversation {i}" };
                _memoryCacheMock.Setup(c => c.Set(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<MemoryCacheEntryOptions>()));
                await _repository.AddAsync(conv);
            }

            // Act
            var page1 = await _repository.GetAllAsync(null, 1, 3);
            var page2 = await _repository.GetAllAsync(null, 2, 3);

            // Assert
            Assert.Equal(3, page1.Count);
            Assert.Equal(3, page2.Count);
            Assert.NotEqual(page1[0].Id, page2[0].Id);
        }

        [Fact]
        public async Task GetUsageStatisticsAsync_WithDateRange_ReturnsStatistics()
        {
            // Arrange
            var fromDate = DateTime.UtcNow.AddDays(-7);
            var toDate = DateTime.UtcNow;

            var conversation = new Conversation
            {
                Id = "stats-conv",
                UserId = "test-user",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3),
                Messages = new List<ConversationMessage>
                {
                    new ConversationMessage { Role = MessageRole.User, TokensUsed = 10, ModelUsed = "gpt-4" },
                    new ConversationMessage { Role = MessageRole.Assistant, TokensUsed = 20, ModelUsed = "gpt-4" },
                    new ConversationMessage { Role = MessageRole.User, TokensUsed = 15, ModelUsed = "gpt-3.5-turbo" }
                }
            };

            _memoryCacheMock.Setup(c => c.Set(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<MemoryCacheEntryOptions>()));
            await _repository.AddAsync(conversation);

            // Act
            var result = await _repository.GetUsageStatisticsAsync(fromDate, toDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalRequests); // 2 user messages
            Assert.Equal(45, result.TotalTokens); // 10 + 20 + 15
            Assert.Equal(2, result.RequestsPerModel.Count);
            Assert.Equal(1, result.RequestsPerModel["gpt-4"]);
            Assert.Equal(1, result.RequestsPerModel["gpt-3.5-turbo"]);
        }

        [Fact]
        public async Task GetMostRecentByUserIdAsync_WithValidUser_ReturnsMostRecent()
        {
            // Arrange
            var userId = "126";
            var oldConv = new Conversation
            {
                Id = "old-conv",
                UserId = userId,
                UpdatedAt = DateTime.UtcNow.AddHours(-2)
            };
            var recentConv = new Conversation
            {
                Id = "recent-conv",
                UserId = userId,
                UpdatedAt = DateTime.UtcNow.AddHours(-1)
            };

            _memoryCacheMock.Setup(c => c.Set(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<MemoryCacheEntryOptions>()));

            await _repository.AddAsync(oldConv);
            await _repository.AddAsync(recentConv);

            // Act
            var result = await _repository.GetMostRecentByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("recent-conv", result.Id);
        }

        [Fact]
        public async Task GetMostRecentByUserIdAsync_WithInvalidUser_ReturnsNull()
        {
            // Arrange
            var invalidUserId = "999";

            // Act
            var result = await _repository.GetMostRecentByUserIdAsync(invalidUserId);

            // Assert
            Assert.Null(result);
        }
    }

    public class CacheServiceTests
    {
        private readonly CacheService _cacheService;
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly Mock<ILogger<CacheService>> _loggerMock;

        public CacheServiceTests()
        {
            _memoryCacheMock = new Mock<IMemoryCache>();
            _loggerMock = new Mock<ILogger<CacheService>>();
            _cacheService = new CacheService(_memoryCacheMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAsync_WithExistingKey_ReturnsValue()
        {
            // Arrange
            var key = "test-key";
            var expectedValue = "test-value";
            object? cachedValue = expectedValue;
            _memoryCacheMock.Setup(c => c.TryGetValue(key, out cachedValue)).Returns(true);

            // Act
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public async Task GetAsync_WithNonExistingKey_ReturnsDefault()
        {
            // Arrange
            var key = "non-existent";
            object? cachedValue = null;
            _memoryCacheMock.Setup(c => c.TryGetValue(key, out cachedValue)).Returns(false);

            // Act
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SetAsync_WithValidData_SetsInCache()
        {
            // Arrange
            var key = "set-key";
            var value = "set-value";
            var expiration = TimeSpan.FromMinutes(5);

            _memoryCacheMock.Setup(c => c.Set(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<MemoryCacheEntryOptions>()));

            // Act
            await _cacheService.SetAsync(key, value, expiration);

            // Assert
            _memoryCacheMock.Verify(c => c.Set(
                It.Is<object>(k => k.ToString() == key),
                It.Is<object>(v => v.ToString() == value),
                It.IsAny<MemoryCacheEntryOptions>()), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingKey_ReturnsTrue()
        {
            // Arrange
            var key = "existing-key";
            object? cachedValue = "some-value";
            _memoryCacheMock.Setup(c => c.TryGetValue(key, out cachedValue)).Returns(true);

            // Act
            var result = await _cacheService.ExistsAsync(key);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingKey_ReturnsFalse()
        {
            // Arrange
            var key = "non-existing-key";
            object? cachedValue = null;
            _memoryCacheMock.Setup(c => c.TryGetValue(key, out cachedValue)).Returns(false);

            // Act
            var result = await _cacheService.ExistsAsync(key);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveAsync_WithValidKey_RemovesFromCache()
        {
            // Arrange
            var key = "remove-key";

            // Act
            await _cacheService.RemoveAsync(key);

            // Assert
            _memoryCacheMock.Verify(c => c.Remove(key), Times.Once);
        }

        [Fact]
        public async Task GetOrCreateAsync_WithExistingValue_ReturnsCachedValue()
        {
            // Arrange
            var key = "existing-key";
            var cachedValue = "cached-data";
            object? outValue = cachedValue;
            _memoryCacheMock.Setup(c => c.TryGetValue(key, out outValue)).Returns(true);

            // Act
            var result = await _cacheService.GetOrCreateAsync(key, TimeSpan.FromMinutes(5), () => Task.FromResult("new-data"));

            // Assert
            Assert.Equal(cachedValue, result);
        }

        [Fact]
        public async Task GetOrCreateAsync_WithNonExistingValue_CreatesAndReturnsNewValue()
        {
            // Arrange
            var key = "new-key";
            var newValue = "new-data";
            object? outValue = null;
            _memoryCacheMock.Setup(c => c.TryGetValue(key, out outValue)).Returns(false);
            _memoryCacheMock.Setup(c => c.Set(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<MemoryCacheEntryOptions>()));

            // Act
            var result = await _cacheService.GetOrCreateAsync(key, TimeSpan.FromMinutes(5), () => Task.FromResult(newValue));

            // Assert
            Assert.Equal(newValue, result);
            _memoryCacheMock.Verify(c => c.Set(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<MemoryCacheEntryOptions>()), Times.Once);
        }
    }
}