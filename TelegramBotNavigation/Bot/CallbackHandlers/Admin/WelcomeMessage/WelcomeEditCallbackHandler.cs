using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services.Sessions;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.WelcomeMessage
{
    public class WelcomeEditCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.WelcomeEdit;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ILocalizationManager _localizer;
        private readonly ITelegramMessageService _messageService;
        private readonly ISessionManager _sessionManager;
        private readonly ILogger<WelcomeEditCallbackHandler> _logger;

        public WelcomeEditCallbackHandler(
            IUserRepository userRepository,
            IUserService userService,
            ILocalizationManager localizer,
            ITelegramMessageService messageService,
            ISessionManager sessionManager,
            ILogger<WelcomeEditCallbackHandler> logger)
        {
            _userRepository = userRepository;
            _userService = userService;
            _localizer = localizer;
            _messageService = messageService;
            _sessionManager = sessionManager;
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

            if (args.Length == 0) return;

            var languageCode = args[0];
            LanguageCode welocmeLanguage = LanguageCodeHelper.FromTelegramTag(languageCode);

            await _sessionManager.ClearSessionAsync(userId);

            await _sessionManager.SetSessionAsync(userId, new SessionData
            {
                Action = SessionKeys.WelcomeEdit,
                Data = new Dictionary<string, string>
                {
                    { "lang", welocmeLanguage.ToLanguageTag() }
                }
            }, TimeSpan.FromMinutes(10));

            var langLabel = welocmeLanguage.GetDisplayLabel();

            var prompt = await _localizer.GetInterfaceTranslation(LocalizationKeys.Messages.WelcomeEditPrompt, user.LanguageCode);
            var promptWithLang = $"{langLabel}\n\n{prompt}";

            var promtTemplate = TelegramTemplate.Create(promptWithLang);
            await _messageService.EditTemplateAsync(chatId, messageId, promtTemplate, ct);
        }
    }
}
