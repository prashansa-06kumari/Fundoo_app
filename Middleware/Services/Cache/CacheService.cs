using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Middleware.Services.Cache
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheService> _logger;

        public CacheService(
            IDistributedCache cache,
            ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                byte[]? data = await _cache.GetAsync(key);

                if (data == null)
                {
                    return default;
                }

                return JsonSerializer.Deserialize<T>(data);
            }
            catch (Exception ex)
            {
                // Memurai/Redis down ho to bhi app chalti rahe
                _logger.LogWarning(ex, "Cache read failed for {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiry)
        {
            try
            {
                await _cache.SetAsync(
                    key,
                    JsonSerializer.SerializeToUtf8Bytes(value),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = expiry
                    });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache write failed for {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache remove failed for {Key}", key);
            }
        }
    }
}