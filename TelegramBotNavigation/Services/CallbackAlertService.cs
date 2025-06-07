using Telegram.Bot;
using TelegramBotNavigation.Services.Interfaces;

namespace TelegramBotNavigation.Services
{
    public class CallbackAlertService : ICallbackAlertService
    {
        private readonly ITelegramBotClient _bot;
        private readonly ILogger<CallbackAlertService> _logger;

        public CallbackAlertService(ITelegramBotClient bot, ILogger<CallbackAlertService> logger)
        {
            _bot = bot;
            _logger = logger;
        }

        public async Task ShowAsync(string CallbackQueryId, string message, bool showAlert = false, string? url = null, CancellationToken cancellationToken = default)
        {
            try
            {
                await _bot.AnswerCallbackQuery(
                    callbackQueryId: CallbackQueryId,
                    text: message,
                    showAlert: showAlert,
                    url: url,
                    cancellationToken: cancellationToken
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to show callback alert: {Message}", message);
            }
        }
    }
}
