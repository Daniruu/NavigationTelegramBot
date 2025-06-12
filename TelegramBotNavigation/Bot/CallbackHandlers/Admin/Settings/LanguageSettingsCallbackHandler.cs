using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Settings
{
    public class LanguageSettingsCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.LanguageSettings;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ILocalizationManager _localizer;
        private readonly ILogger<LanguageSettingsCallbackHandler> _logger;
        private readonly ICallbackAlertService _callbackAlertService;
        private readonly ITelegramMessageService _messageService;
        private readonly ILanguageSettingRepository _languageSettingRepository;

        public LanguageSettingsCallbackHandler(IUserRepository userRepository, IUserService userService, ILocalizationManager localizer, ILogger<LanguageSettingsCallbackHandler> logger, ICallbackAlertService callbackAlertService, ITelegramMessageService messageService, ILanguageSettingRepository languageSettingRepository)
        {
            _userRepository = userRepository;
            _userService = userService;
            _localizer = localizer;
            _logger = logger;
            _callbackAlertService = callbackAlertService;
            _messageService = messageService;
            _languageSettingRepository = languageSettingRepository;
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

            LanguageSetting? selectedLanguage = null;
            if (args.Length > 0 && int.TryParse(args[0], out var languageId))
            {
                selectedLanguage = await _languageSettingRepository.GetByIdAsync(languageId);
            }

            var template = await LanguageSettingsTemplate.CreateAsync(user.LanguageCode, _localizer, _languageSettingRepository, selectedLanguage);
            await _messageService.EditTemplateAsync(chatId, messageId, template, ct);
        }
    }
}
