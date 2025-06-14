﻿namespace TelegramBotNavigation.Services.Sessions
{
    public interface ISessionManager
    {
        Task SetSessionAsync(long userId, SessionData data, TimeSpan? ttl = null);
        Task<SessionData?> GetSessionAsync(long userId);
        Task ClearSessionAsync(long userId);
    }
}
