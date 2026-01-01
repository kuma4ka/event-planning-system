using EventPlanning.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace EventPlanning.Infrastructure.Services;

public class MemoryCacheService(IMemoryCache memoryCache) : ICacheService
{
    public T? Get<T>(string key)
    {
        return memoryCache.TryGetValue(key, out T? value) ? value : default;
    }

    public void Set<T>(string key, T value, TimeSpan? slidingExpiration = null, TimeSpan? absoluteExpiration = null)
    {
        var options = new MemoryCacheEntryOptions();
        
        if (slidingExpiration.HasValue)
        {
            options.SetSlidingExpiration(slidingExpiration.Value);
        }

        if (absoluteExpiration.HasValue)
        {
            options.SetAbsoluteExpiration(absoluteExpiration.Value);
        }

        // Default fallback if nothing specified
        if (!slidingExpiration.HasValue && !absoluteExpiration.HasValue)
        {
             options.SetSlidingExpiration(TimeSpan.FromMinutes(10));
        }

        memoryCache.Set(key, value, options);
    }

    public void Remove(string key)
    {
        memoryCache.Remove(key);
    }
}
