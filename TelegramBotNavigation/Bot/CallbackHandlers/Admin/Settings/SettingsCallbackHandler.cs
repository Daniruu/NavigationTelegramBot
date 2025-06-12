using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;
using TelegramBotNavigation.Services;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Settings
{
    public class SettingsCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.Settings;

        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly ILogger<SettingsCallbackHandler> _logger;
        private readonly ICallbackAlertService _callbackAlertService;

        public SettingsCallbackHandler(
            IUserService userService,
            IUserRepository userRepository,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            ILogger<SettingsCallbackHandler> logger,
            ICallbackAlertService callbackAlertService)
        {
            _userService = userService;
            _userRepository = userRepository;
            _messageService = messageService;
            _localizer = localizer;
            _logger = logger;
            _callbackAlertService = callbackAlertService;
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
                _logger.LogWarning("Access denied for user {UserId} while trying to access admin callback.", userId);
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.NotAdmin, user.LanguageCode);
                await _callbackAlertService.ShowAsync(query.Id, errorMessage, showAlert: true, cancellationToken: ct);
                return;
            }

            var template = await SettingsTemplate.CreateAsync(user.LanguageCode, _localizer);
            await _messageService.EditTemplateAsync(chatId, messageId, template, ct);
        }
    }
}
