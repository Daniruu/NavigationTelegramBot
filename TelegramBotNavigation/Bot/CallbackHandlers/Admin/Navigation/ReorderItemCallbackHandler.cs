using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services.Sessions;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Navigation
{
    public class ReorderItemCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.ReorderItem;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ILogger<ReorderSelectItemCallbackHandler> _logger;
        private readonly ILocalizationManager _localizer;
        private readonly ITelegramMessageService _messageService;
        private readonly IReorderSessionManager _reorderSessionManager;

        public ReorderItemCallbackHandler(
            IUserRepository userRepository,
            IUserService userService,
            ILogger<ReorderSelectItemCallbackHandler> logger,
            ILocalizationManager localizer, ITelegramMessageService messageService,
            IReorderSessionManager reorderSessionManager)
        {
            _userRepository = userRepository;
            _userService = userService;
            _logger = logger;
            _localizer = localizer;
            _messageService = messageService;
            _reorderSessionManager = reorderSessionManager;
        }

        public async Task HandleAsync(CallbackQuery query, string[] args, CancellationToken ct)
        {
            var chatId = query.Message!.Chat.Id;
            var messageId = query.Message.MessageId;
            var userId = query.From.Id;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            if (!_userService.IsAdmin(user))
            {
                _logger.LogWarning("User {UserId} not found when trying to access admin command.", userId);
                var errorMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.NotAdmin, user.LanguageCode);
                var errorTemplate = TelegramTemplate.Create(errorMessage);
                await _messageService.SendTemplateAsync(chatId, errorTemplate, ct);
                return;
            }

            if (args.Length == 0) return;

            var direction = args[0];
            if (string.IsNullOrWhiteSpace(direction)) return;

            var session = await _reorderSessionManager.GetAsync(userId);
            if (session == null)
            {
                _logger.LogWarning("No active reorder session found.");
                var error = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.SessionDataMissing, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(error), ct);
                return;
            }

            await _reorderSessionManager.MoveItemAsync(userId, direction);

            var template = await NavigationReorderTemplate.CreateAsync(user.LanguageCode, _localizer, session);
            await _messageService.EditTemplateAsync(chatId, messageId, template, ct);
        }
    }
}
