using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services.Sessions;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Navigation
{
    public class SelectItemTypeCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.NavigationItemSelectType;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ILogger<SelectItemTypeCallbackHandler> _logger;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly ISessionManager _sessionManager;
        private readonly ICallbackAlertService _callbackAlertService;

        public SelectItemTypeCallbackHandler(
            IUserRepository userRepository,
            IUserService userService,
            ILogger<SelectItemTypeCallbackHandler> logger,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            ISessionManager sessionManager,
            ICallbackAlertService callbackAlertService)
        {
            _userRepository = userRepository;
            _userService = userService;
            _logger = logger;
            _messageService = messageService;
            _localizer = localizer;
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

            if (!_userService.IsAdmin(user))
            {
                _logger.LogWarning("User {UserId} not found when trying to access admin command.", userId);
                var errorMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.NotAdmin, user.LanguageCode);
                await _callbackAlertService.ShowAsync(query.Id, errorMessage, cancellationToken: ct);
                return;
            }

            if (args.Length == 0 || !Enum.TryParse<MenuActionType>(args[0], out var actionType))
            {
                _logger.LogWarning("Invalid or missing action type in callback.");
                var errorMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.InvalidActionType, user.LanguageCode);
                await _callbackAlertService.ShowAsync(query.Id, errorMessage, cancellationToken: ct);
                return;
            }

            var session = await _sessionManager.GetSessionAsync(userId);
            if (session == null || !session.Data.TryGetValue("menuId", out var menuId) || !session.Data.TryGetValue("lang", out var lang))
            {
                _logger.LogWarning("No active session or missing menuId.");
                var errorMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.SessionDataMissing, user.LanguageCode);
                await _callbackAlertService.ShowAsync(query.Id, errorMessage, cancellationToken: ct);
                return;
            }

            session.Data["type"] = actionType.ToString();
            session.Action = SessionKeys.NavigationItemAddLabel;

            await _sessionManager.SetSessionAsync(userId, session, TimeSpan.FromMinutes(10));

            var langCode = LanguageCodeHelper.FromTelegramTag(lang);

            var prompt = await _localizer.GetInterfaceTranslation(LocalizationKeys.Messages.EnterButtonLabel, user.LanguageCode, langCode.GetDisplayLabel());
            await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(prompt), ct);
        }
    }
}
