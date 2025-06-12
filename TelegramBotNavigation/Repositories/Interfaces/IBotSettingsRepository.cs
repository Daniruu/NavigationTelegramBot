namespace TelegramBotNavigation.Repositories.Interfaces
{
    public interface IBotSettingsRepository
    {
        Task<string?> GetValueAsync(string key);
        Task SetValueAsync(string key, string value);
    }
}
