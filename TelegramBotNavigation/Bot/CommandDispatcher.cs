using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.CommandHandlers;

namespace TelegramBotNavigation.Bot
{
    public class CommandDispatcher
    {
        private readonly IEnumerable<ICommandHandler> _handlers;
        private readonly ILogger<CommandDispatcher> _logger;

        public CommandDispatcher(IEnumerable<ICommandHandler> handlers, ILogger<CommandDispatcher> logger)
        {
            _handlers = handlers;
            _logger = logger;
        }

        public async Task DispatchAsync(Message message, CancellationToken ct)
        {
            var commandText = message.Text?.Split(' ')[0];
            _logger.LogInformation($"Received command: {commandText}");

            var handler = _handlers.FirstOrDefault(h => h.Command.Equals(commandText, StringComparison.OrdinalIgnoreCase));

            if (handler != null)
            {
                _logger.LogInformation($"Dispatch command to: {handler.GetType().Name}");
                try
                {
                    await handler.HandleAsync(message, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error handling command: {commandText} by {handler.GetType().Name}");
                }

            }
            else
            {
                _logger.LogWarning($"No handler found for command: {commandText}");
            }
        }
    }
}
