using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services.Sessions;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;
using static TelegramBotNavigation.Bot.Shared.CallbackKeys;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Navigation
{
    public class NavigationAddCallbackHandler : ICallbackHandler
    {
        public string Key => NavigationAdd;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ILogger<NavigationManageCallbackHandler> _logger;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly ISessionManager _sessionManager;
        private readonly ICallbackAlertService _callbackAlertService;

        public NavigationAddCallbackHandler(
            IUserRepository userRepository,
            IUserService userService,
            ILogger<NavigationManageCallbackHandler> logger,
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
            var userId = query.From.Id;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            if (!_userService.IsAdmin(user))
            {
                _logger.LogWarning("User {UserId} not found when trying to access admin command.", userId);
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.NotAdmin, user.LanguageCode);
                await _callbackAlertService.ShowAsync(query.Id, errorMessage, cancellationToken: ct);
                return;
            }

            await _sessionManager.ClearSessionAsync(userId);

            await _sessionManager.SetSessionAsync(userId, new SessionData
            {
                Action = SessionKeys.NavigationAdd,
                Data = new Dictionary<string, string>()
            }, TimeSpan.FromMinutes(10));

            var prompt = await _localizer.GetInterfaceTranslation(Messages.NavigationAddPrompt, user.LanguageCode);
            var promptTemplate = TelegramTemplate.Create(prompt);

            await _messageService.SendTemplateAsync(chatId, promptTemplate, ct);
        }
    }
}
