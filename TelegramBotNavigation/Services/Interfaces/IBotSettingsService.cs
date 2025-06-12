namespace TelegramBotNavigation.Services.Interfaces
{
    public interface IBotSettingsService
    {
        Task<string?> GetAsync(string key);
        Task SetAsync(string key, string value);

        Task<long?> GetSupportGroupIdAsync();
        Task SetSupportGroupIdAsync(long chatId);

        Task<bool> GetShowWelcomeMessageAsync();
        Task SetShowWelcomeMessageAsync(bool value);

        Task<bool> GetNotifySupportRequestsAsync();
        Task SetNotifySupportRequestsAsync(bool value);
    }
}
