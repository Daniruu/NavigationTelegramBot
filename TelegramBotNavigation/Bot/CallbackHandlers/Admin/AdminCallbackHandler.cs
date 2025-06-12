using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services.Sessions;
using TelegramBotNavigation.Services;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin
{
    public class AdminCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.AdminPanel;

        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly ILogger<AdminCallbackHandler> _logger;
        private readonly ISessionManager _sessionManager;
        private readonly ICallbackAlertService _callbackAlertService;

        public AdminCallbackHandler(
            IUserService userService,
            IUserRepository userRepository,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            ILogger<AdminCallbackHandler> logger,
            ISessionManager sessionManager,
            ICallbackAlertService callbackAlertService)
        {
            _userService = userService;
            _userRepository = userRepository;
            _messageService = messageService;
            _localizer = localizer;
            _logger = logger;
            _sessionManager = sessionManager;
            _callbackAlertService = callbackAlertService;
        }

        public async Task HandleAsync(CallbackQuery query, string[] args, CancellationToken ct)
        {
            var chatId = query.Message!.Chat.Id;
            var messageId = query.Message.MessageId;
            var userId = query.From.Id;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            await _sessionManager.ClearSessionAsync(userId);

            if (!_userService.IsAdmin(user))
            {
                _logger.LogWarning("User {UserId} not found when trying to access admin command.", userId);
                var errorMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.NotAdmin, user.LanguageCode);
                await _callbackAlertService.ShowAsync(query.Id, errorMessage, showAlert: true, cancellationToken: ct);
                return;
            }

            var template = await AdminPanelMainTemplate.CreateAsync(user.LanguageCode, _localizer);
            await _messageService.EditTemplateAsync(chatId, messageId, template, ct);
        }
    }
}
