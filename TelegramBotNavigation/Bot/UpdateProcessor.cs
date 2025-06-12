using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services.Sessions;

namespace TelegramBotNavigation.Bot
{
    public class UpdateProcessor : IUpdateProcessor
    {
        private readonly ILogger<UpdateProcessor> _logger;
        private readonly CommandDispatcher _commandDispatcher;
        private readonly CallbackDispatcher _callbackDispatcher;
        private readonly SessionDispatcher _sessionDispatcher;
        private readonly MessageDispatcher _messageDispatcher;
        private readonly ISessionManager _sessionManager;
        private readonly ISupportRequestService _supportService;

        public UpdateProcessor(
            ILogger<UpdateProcessor> logger, 
            CommandDispatcher commandDispatcher, 
            CallbackDispatcher callbackDispatcher, 
            SessionDispatcher sessionDispatcher,
            MessageDispatcher messageDispatcher,
            ISessionManager sessionManager,
            ISupportRequestService supportService)
        {
            _logger = logger;
            _commandDispatcher = commandDispatcher;
            _callbackDispatcher = callbackDispatcher;
            _sessionDispatcher = sessionDispatcher;
            _messageDispatcher = messageDispatcher;
            _sessionManager = sessionManager;
            _supportService = supportService;
        }

        public async Task HandleAsync(Update update, CancellationToken ct)
        {
            _logger.LogInformation("Received update: {Type}", update.Type);

            switch (update.Type)
            {
                case UpdateType.Message:
                    if (update.Message != null && update.Message.From != null)
                    {
                        var userId = update.Message.From!.Id;

                        if (!string.IsNullOrWhiteSpace(update.Message.Text) && update.Message.Text.StartsWith("/"))
                        {
                            await _sessionManager.ClearSessionAsync(userId);
                            await _commandDispatcher.DispatchAsync(update.Message, ct);
                        }
                        else
                        {
                            var session = await _sessionManager.GetSessionAsync(userId);
                            if (session != null && update.Message.Chat.Type == ChatType.Private)
                            {
                                await _sessionDispatcher.DispatchAsync(update.Message, session, ct);
                            }
                            else
                            {
                                await _messageDispatcher.DispatchAsync(update.Message, ct);
                            }
                        }
                    }
                    break;

                case UpdateType.CallbackQuery:
                    if (update.CallbackQuery != null)
                    {
                        await _callbackDispatcher.DispatchAsync(update.CallbackQuery, ct);
                    }
                    break;

                default:
                    _logger.LogWarning("Unhandled update type: {Type}", update.Type);
                    break;
            }
        }
    }
}
