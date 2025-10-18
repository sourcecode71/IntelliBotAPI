using IntelliBot.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace IntelliBot.Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheService> _logger;
        private readonly MemoryCacheEntryOptions _defaultCacheOptions;

        public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;

            _defaultCacheOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(30),
                Size = 1 // Relative size for memory management
            };
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out T? value))
                {
                    _logger.LogDebug("Cache hit for key {CacheKey}", key);
                    return await Task.FromResult(value);
                }

                _logger.LogDebug("Cache miss for key {CacheKey}", key);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache entry for key {CacheKey}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            try
            {
                var options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration,
                    Size = 1
                };

                _memoryCache.Set(key, value, options);
                _logger.LogDebug("Cache set for key {CacheKey} with {Expiration} expiration", key, expiration);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache entry for key {CacheKey}", key);
                // Don't throw - caching failures shouldn't break the application
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var exists = _memoryCache.TryGetValue(key, out _);
                _logger.LogDebug("Cache existence check for key {CacheKey}: {Exists}", key, exists);

                return await Task.FromResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache existence for key {CacheKey}", key);
                return false;
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                _logger.LogDebug("Cache removed for key {CacheKey}", key);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache entry for key {CacheKey}", key);
                // Don't throw - caching failures shouldn't break the application
            }
        }

        // Additional utility methods
        public async Task<T> GetOrCreateAsync<T>(string key, TimeSpan expiration, Func<Task<T>> factory)
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out T? cachedValue) && cachedValue != null)
                {
                    return cachedValue;
                }

                var value = await factory();
                await SetAsync(key, value, expiration);
                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrCreate for key {CacheKey}", key);
                return await factory(); // Fallback to factory without caching
            }
        }
    }
}