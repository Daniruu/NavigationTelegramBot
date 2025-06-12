using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services.Sessions;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Support
{
    public class SupportReplyCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.SupportReply;

        private readonly ISessionManager _sessionManager;
        private readonly ITelegramMessageService _messageService;
        private readonly ISupportRequestService _supportService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<SupportReplyCallbackHandler> _logger;
        private readonly IUserService _userService;
        private readonly ICallbackAlertService _callbackAlertService;
        private readonly ILocalizationManager _localizer;

        public SupportReplyCallbackHandler(
        ISessionManager sessionManager,
        ITelegramMessageService messageService,
        ISupportRequestService supportService,
        IUserRepository userRepository,
        ILogger<SupportReplyCallbackHandler> logger,
        IUserService userService,
        ICallbackAlertService callbackAlertService,
        ILocalizationManager localizer)
        {
            _sessionManager = sessionManager;
            _messageService = messageService;
            _supportService = supportService;
            _userRepository = userRepository;
            _logger = logger;
            _userService = userService;
            _callbackAlertService = callbackAlertService;
            _localizer = localizer;
        }

        public async Task HandleAsync(CallbackQuery query, string[] args, CancellationToken ct)
        {

            var adminId = query.From.Id;

            if (args.Length == 0 || !long.TryParse(args[0], out var targetUserId))
            {
                return;
            }

            var admin = await _userRepository.GetByIdAsync(adminId);
            if (admin == null) return;

            if (!_userService.IsAdmin(admin))
            {
                _logger.LogWarning("Access denied for user {UserId} while trying to access admin callback.", adminId);
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.NotAdmin, admin.LanguageCode);
                await _callbackAlertService.ShowAsync(query.Id, errorMessage, showAlert: true, cancellationToken: ct);
                return;
            }

            await _sessionManager.ClearSessionAsync(adminId);

            await _sessionManager.SetSessionAsync(adminId, new SessionData
            {
                Action = SessionKeys.SupportReply,
                Data = new Dictionary<string, string>
                {
                    { "userId", targetUserId.ToString() }
                }
            }, TimeSpan.FromMinutes(10));

            var prompt = await _localizer.GetInterfaceTranslation(Messages.EnterSupportRequestReply, admin.LanguageCode, targetUserId);
            await _callbackAlertService.ShowAsync(query.Id, prompt, false, cancellationToken: ct);
        }
    }
}
