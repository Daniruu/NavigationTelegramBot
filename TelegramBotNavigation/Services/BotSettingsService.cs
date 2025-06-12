using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;

namespace TelegramBotNavigation.Services
{
    public class BotSettingsService : IBotSettingsService
    {
        private readonly IBotSettingsRepository _repository;
        private readonly ILogger<BotSettingsService> _logger;

        public BotSettingsService(IBotSettingsRepository repository, ILogger<BotSettingsService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<string?> GetAsync(string key)
        {
            return await _repository.GetValueAsync(key);
        }

        public async Task SetAsync(string key, string value)
        {
            await _repository.SetValueAsync(key, value);
        }

        public async Task<long?> GetSupportGroupIdAsync()
        {
            var val = await GetAsync("SupportGroupId");
            return long.TryParse(val, out var result) ? result : null;
        }

        public Task SetSupportGroupIdAsync(long chatId)
            => SetAsync("SupportGroupId", chatId.ToString());

        public async Task<bool> GetShowWelcomeMessageAsync()
        {
            var val = await GetAsync("ShowWelcomeMessage");
            return bool.TryParse(val, out var result) && result;
        }

        public Task SetShowWelcomeMessageAsync(bool value)
            => SetAsync("ShowWelcomeMessage", value.ToString());

        public async Task<bool> GetNotifySupportRequestsAsync()
        {
            var val = await GetAsync("NotifySupportRequests");
            return bool.TryParse(val, out var result) && result;
        }

        public Task SetNotifySupportRequestsAsync(bool value)
            => SetAsync("NotifySupportRequests", value.ToString());
    }
}
