using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;
using static TelegramBotNavigation.Bot.Shared.CallbackKeys;
using TelegramBotNavigation.Services.Sessions;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Navigation
{
    public class NavigationManageCallbackHandler : ICallbackHandler
    {
        public string Key => NavigationManage;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ILogger<NavigationManageCallbackHandler> _logger;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly IMenuRepository _menuRepository;
        private readonly ISessionManager _sessionManager;
        private readonly ICallbackAlertService _callbackAlertService;

        public NavigationManageCallbackHandler(
            IUserRepository userRepository, 
            IUserService userService,
            ILogger<NavigationManageCallbackHandler> logger,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            IMenuRepository menuRepository,
            ISessionManager sessionManager,
            ICallbackAlertService callbackAlertService)
        {
            _userRepository = userRepository;
            _userService = userService;
            _logger = logger;
            _messageService = messageService;
            _localizer = localizer;
            _menuRepository = menuRepository;
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
                _logger.LogWarning("Access denied for user {UserId} when trying to access admin callback.", userId);
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.NotAdmin, user.LanguageCode);
                await _callbackAlertService.ShowAsync(query.Id, errorMessage, cancellationToken: ct);
                return;
            }

            var menus = await _menuRepository.GetTopLevelMenusAsync();
            var template = await NavigationManageTemplate.CreateAsync(user.LanguageCode, _localizer, menus);
            await _messageService.EditTemplateAsync(chatId, messageId, template, ct);
        }
    }
}
