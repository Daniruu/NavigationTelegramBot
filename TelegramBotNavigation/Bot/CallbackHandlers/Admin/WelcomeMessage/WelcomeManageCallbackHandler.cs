using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.WelcomeMessage
{
    public class WelcomeManageCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.WelcomeManage;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ILocalizationManager _localizer;
        private readonly ITelegramMessageService _messageService;
        private readonly ILanguageSettingRepository _languageSettingRepository;
        private readonly IWelcomeMessageProvider _welcomeMessageProvider;
        private readonly ILogger<WelcomeManageCallbackHandler> _logger;

        public WelcomeManageCallbackHandler(
            IUserRepository userRepository,
            IUserService userService,
            ILocalizationManager localizer,
            ITelegramMessageService messageService,
            ILanguageSettingRepository languageSettingRepository,
            IWelcomeMessageProvider welcomeMessageProvider,
            ILogger<WelcomeManageCallbackHandler> logger)
        {
            _userRepository = userRepository;
            _userService = userService;
            _localizer = localizer;
            _messageService = messageService;
            _languageSettingRepository = languageSettingRepository;
            _welcomeMessageProvider = welcomeMessageProvider;
            _logger = logger;
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

            LanguageCode languageCode;

            if (args.Length == 0)
            {
                var fallbackOrder = await _languageSettingRepository.GetFallbackOrderAsync();
                languageCode = fallbackOrder.First();
            }
            else
            {
                var language = args[0];
                languageCode = LanguageCodeHelper.FromTelegramTag(language);
            }
            

            var template = await WelcomeManageTemplate.CreateAsync(user.LanguageCode, languageCode, _localizer, _languageSettingRepository, _welcomeMessageProvider);
            await _messageService.EditTemplateAsync(chatId, messageId, template, ct);
        }
    }
}
