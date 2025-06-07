using Microsoft.Extensions.Caching.Memory;

namespace TelegramBotNavigation.Services.Sessions
{
    public class MemorySessionManager : ISessionManager
    {
        private readonly IMemoryCache _cache;

        public MemorySessionManager(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task SetSessionAsync(long userId, SessionData data, TimeSpan ttl)
        {
            _cache.Set(userId,data, ttl);
            return Task.CompletedTask;
        }

        public Task<SessionData?> GetSessionAsync(long userId)
        {
            _cache.TryGetValue(userId, out SessionData? data);
            return Task.FromResult(data);
        }

        public Task ClearSessionAsync(long userId)
        {
            _cache.Remove(userId);
            return Task.CompletedTask;
        }
    }
}
