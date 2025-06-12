using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.MessageHandlers;

namespace TelegramBotNavigation.Bot
{
    public class MessageDispatcher
    {
        private readonly IEnumerable<IMessageHandler> _handlers;
        private readonly ILogger<MessageDispatcher> _logger;

        public MessageDispatcher(IEnumerable<IMessageHandler> handlers, ILogger<MessageDispatcher> logger)
        {
            _handlers = handlers;
            _logger = logger;
        }

        public async Task DispatchAsync(Message message, CancellationToken ct)
        {
            _logger.LogInformation("Dispatching message from {UserId} in chat {ChatId}", message.From?.Id, message.Chat.Id);

            foreach (var handler in _handlers)
            {
                if (handler.CanHandle(message))
                {
                    _logger.LogInformation("Handling message with {Handler}", handler.GetType().Name);
                    try
                    {
                        await handler.HandleAsync(message, ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in {Handler} while handling message", handler.GetType().Name);
                    }
                    break;
                }
            }
        }
    }
}
