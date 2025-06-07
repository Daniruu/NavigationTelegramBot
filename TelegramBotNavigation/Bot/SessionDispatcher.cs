using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.SessionHandlers;
using TelegramBotNavigation.Services.Sessions;

namespace TelegramBotNavigation.Bot
{
    public class SessionDispatcher
    {
        private readonly IEnumerable<ISessionHandler> _handlers;
        private readonly ILogger<SessionDispatcher> _logger;

        public SessionDispatcher(IEnumerable<ISessionHandler> handlers, ILogger<SessionDispatcher> logger)
        {
            _handlers = handlers;
            _logger = logger;
        }

        public async Task DispatchAsync(Message message, SessionData session, CancellationToken ct)
        {
            _logger.LogInformation($"Received session action: {session.Action}");

            var handler = _handlers.FirstOrDefault(h => h.Action == session.Action);
            if (handler != null)
            {
                _logger.LogInformation($"Dispatch session to: {handler.GetType().Name}");
                try
                {
                    await handler.HandleAsync(message, session, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error handling session action: {session.Action} by {handler.GetType().Name}");
                }
            }
        }
    }
}
