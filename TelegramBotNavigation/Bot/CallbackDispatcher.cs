using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.CallbackHandlers;

namespace TelegramBotNavigation.Bot
{
    public class CallbackDispatcher
    {
        private readonly IEnumerable<ICallbackHandler> _handlers;
        private readonly ILogger<CallbackDispatcher> _logger;

        public CallbackDispatcher(IEnumerable<ICallbackHandler> handlers, ILogger<CallbackDispatcher> logger)
        {
            _handlers = handlers;
            _logger = logger;
        }

        public async Task DispatchAsync(CallbackQuery query, CancellationToken ct)
        {
            var parts = query.Data?.Split(':') ?? Array.Empty<string>();
            _logger.LogInformation($"Received callback: {query.Data}");

            if (parts.Length == 0)
            {
                _logger.LogWarning("Empty callback data");
                return;
            }

            var key = parts[0];
            var args = parts.Skip(1).ToArray();

            var handler = _handlers.FirstOrDefault(h => h.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (handler != null)
            {
                _logger.LogInformation($"Dispatch callback to: {handler.GetType().Name}");
                try
                {
                    await handler.HandleAsync(query, args, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error handling callback: {key} by {handler.GetType().Name}");
                }
            }
            else
            {
                _logger.LogWarning($"No handler found for callback: {key}");
            }
        }
    }
}
