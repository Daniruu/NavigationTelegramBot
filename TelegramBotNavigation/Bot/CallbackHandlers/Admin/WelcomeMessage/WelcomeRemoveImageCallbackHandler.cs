using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Repositories;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services.Sessions;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.WelcomeMessage
{
    public class WelcomeRemoveImageCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.WelcomeRemoveImage;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ITelegramMessageService _messageService;
        private readonly IWelcomeMessageRepository _welcomeMessageRepository;
        private readonly ILocalizationManager _localizer;
        private readonly ISessionManager _sessionManager;
        private readonly ILogger<WelcomeRemoveImageCallbackHandler> _logger;
        private readonly ILanguageSettingRepository _languageSettingRepository;
        private readonly IWelcomeMessageProvider _welcomeMessageProvider;
        private readonly ICallbackAlertService _callbackAlertService;

        public WelcomeRemoveImageCallbackHandler(
            IUserRepository userRepository,
            IUserService userService,
            ITelegramMessageService messageService,
            IWelcomeMessageRepository welcomeMessageRepository,
            ILocalizationManager localizer,
            ISessionManager sessionManager,
            ILogger<WelcomeRemoveImageCallbackHandler> logger,
            ILanguageSettingRepository languageSettingRepository,
            IWelcomeMessageProvider welcomeMessageProvider,
            ICallbackAlertService callbackAlertService)
        {
            _userRepository = userRepository;
            _userService = userService;
            _messageService = messageService;
            _welcomeMessageRepository = welcomeMessageRepository;
            _localizer = localizer;
            _sessionManager = sessionManager;
            _logger = logger;
            _languageSettingRepository = languageSettingRepository;
            _welcomeMessageProvider = welcomeMessageProvider;
            _callbackAlertService = callbackAlertService;
        }

        public async Task HandleAsync(CallbackQuery query, string[] args, CancellationToken ct)
        {
            var chatId = query.Message!.Chat.Id;
            var userId = query.From.Id;
            var messageId = query.Message.MessageId;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            if (!_userService.IsAdmin(user))
            {
                _logger.LogWarning("User {UserId} not found when trying to access admin command.", userId);
                var errorMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.NotAdmin, user.LanguageCode);
                await _callbackAlertService.ShowAsync(query.Id, errorMessage, cancellationToken: ct);
                return;
            }

            if (args.Length == 0) return;

            var language = args[0];
            LanguageCode languageCode = LanguageCodeHelper.FromTelegramTag(language);

            await _sessionManager.ClearSessionAsync(userId);

            await _welcomeMessageRepository.RemoveImageAsync(languageCode);

            var success = await _localizer.GetInterfaceTranslation(LocalizationKeys.Notifications.WelcomeImageRemoved, user.LanguageCode);
            await _callbackAlertService.ShowAsync(query.Id, success, cancellationToken: ct);

            var template = await WelcomeManageTemplate.CreateAsync(user.LanguageCode, languageCode, _localizer, _languageSettingRepository, _welcomeMessageProvider);
            await _messageService.SendTemplateAsync(chatId, template, ct);
        }
    }
}
