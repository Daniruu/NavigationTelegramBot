namespace TelegramBotNavigation.Services.Sessions
{
    public interface IReorderSessionManager
    {
        Task StartAsync(long userId, MenuReorderSession session);
        Task<MenuReorderSession?> GetAsync(long userId);
        Task UpdateAsync(long userId, MenuReorderSession session);
        Task ClearAsync(long userId);
        Task MoveItemAsync(long userId, string direction);
    }
}
