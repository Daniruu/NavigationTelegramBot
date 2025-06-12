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
            var parts = message.Text.Split(' ');
            var command = parts[0].ToLower();
            var args = parts.Skip(1).ToArray();

            _logger.LogInformation($"Received command: {command}");

            var handler = _handlers.FirstOrDefault(h => h.Command.Equals(command, StringComparison.OrdinalIgnoreCase));

            if (handler != null)
            {
                _logger.LogInformation($"Dispatch command to: {handler.GetType().Name}");
                try
                {
                    await handler.HandleAsync(message, args, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error handling command: {command} by {handler.GetType().Name}");
                }

            }
            else
            {
                _logger.LogWarning($"No handler found for command: {command}");
            }
        }
    }
}
