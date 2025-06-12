using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Settings
{
    public class LanguageMoveCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.LanguageMove;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ILocalizationManager _localizer;
        private readonly ILogger<LanguageMoveCallbackHandler> _logger;
        private readonly ICallbackAlertService _callbackAlertService;
        private readonly ITelegramMessageService _messageService;
        private readonly ILanguageSettingRepository _languageSettingRepository;

        public LanguageMoveCallbackHandler(IUserRepository userRepository, IUserService userService, ILocalizationManager localizer, ILogger<LanguageMoveCallbackHandler> logger, ICallbackAlertService callbackAlertService, ITelegramMessageService messageService, ILanguageSettingRepository languageSettingRepository)
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
            if (args.Length < 2 || !int.TryParse(args[0], out var languageId)) return;

            var direction = args[1];

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

            var languages = await _languageSettingRepository.GetLanguageSettingsAsync();

            var selectedLanguage = languages.FirstOrDefault(l => l.Id == languageId);
            if (selectedLanguage == null) return;

            var index = languages.IndexOf(selectedLanguage);
            var swapIndex = direction.ToLower() switch
            {
                "up" when index > 0 => index - 1,
                "down" when index < languages.Count - 1 => index + 1,
                _ => -1
            };

            if (swapIndex == -1) return;

            var target = languages[swapIndex];
            var temp = selectedLanguage.Priority;
            selectedLanguage.Priority = target.Priority;
            target.Priority = temp;

            await _languageSettingRepository.UpdateAsync(selectedLanguage);
            await _languageSettingRepository.UpdateAsync(target);

            var template = await LanguageSettingsTemplate.CreateAsync(user.LanguageCode, _localizer, _languageSettingRepository, selectedLanguage);
            await _messageService.EditTemplateAsync(chatId, messageId, template, ct);
        }
    }
}
