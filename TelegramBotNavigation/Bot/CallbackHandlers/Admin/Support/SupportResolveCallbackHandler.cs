using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;
using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services.Sessions;
using TelegramBotNavigation.Enums;
using Telegram.Bot.Requests.Abstractions;
using TelegramBotNavigation.Bot.Templates;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Support
{
    public class SupportResolveCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.SupportResolve;

        private readonly ISessionManager _sessionManager;
        private readonly ITelegramMessageService _messageService;
        private readonly ISupportRequestService _supportService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<SupportResolveCallbackHandler> _logger;
        private readonly IUserService _userService;
        private readonly ICallbackAlertService _callbackAlertService;
        private readonly ILocalizationManager _localizer;
        private readonly IBotSettingsService _settingsService;
        private readonly ITelegramClient _telegramClient;

        public SupportResolveCallbackHandler(
        ISessionManager sessionManager,
        ITelegramMessageService messageService,
        ISupportRequestService supportService,
        IUserRepository userRepository,
        ILogger<SupportResolveCallbackHandler> logger,
        IUserService userService,
        ICallbackAlertService callbackAlertService,
        ILocalizationManager localizer,
        IBotSettingsService settingsService,
        ITelegramClient telegramClient)
        {
            _sessionManager = sessionManager;
            _messageService = messageService;
            _supportService = supportService;
            _userRepository = userRepository;
            _logger = logger;
            _userService = userService;
            _callbackAlertService = callbackAlertService;
            _localizer = localizer;
            _settingsService = settingsService;
            _telegramClient = telegramClient;
        }

        public async Task HandleAsync(CallbackQuery query, string[] args, CancellationToken ct)
        {

            var adminId = query.From.Id;

            if (args.Length == 0 || !int.TryParse(args[0], out var requestId)) return;

            var request = await _supportService.GetByIdAsync(requestId);
            if (request == null) return;

            var admin = await _userRepository.GetByIdAsync(adminId);
            if (admin == null) return;

            if (!_userService.IsAdmin(admin))
            {
                _logger.LogWarning("Access denied for user {UserId} while trying to access admin callback.", adminId);
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.NotAdmin, admin.LanguageCode);
                await _callbackAlertService.ShowAsync(query.Id, errorMessage, showAlert: true, cancellationToken: ct);
                return;
            }

            try
            {
                await _supportService.CloseRequestAsync(requestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while closing request {RequestId}", requestId);
                return;
            }

            await _sessionManager.ClearSessionAsync(adminId);

            var userSession = await _sessionManager.GetSessionAsync(request.UserId);
            if (userSession != null && userSession.Action == SessionKeys.SupportRequest)
            {
                await _sessionManager.ClearSessionAsync(request.UserId);
            }

            if (request.TopicId.HasValue)
            {
                try
                {
                    var chatId = await _settingsService.GetSupportGroupIdAsync();
                    if (chatId.HasValue)
                    {
                        await _telegramClient.EditForumTopicIconAsync(chatId.Value, request.TopicId.Value, "✅", ct);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to edit/close forum topic for request {RequestId}", request.Id);
                }
            }

            if (request.AdminMessageId.HasValue)
            {
                try
                {
                    var chatId = await _settingsService.GetSupportGroupIdAsync();
                    if (chatId.HasValue)
                    {
                        var updatedText = await _localizer.GetInterfaceTranslation(
                            Notifications.SupportRequestResolvedAdmin, Enums.LanguageCode.Ru, 
                            request.User.FirstName, request.User.LastName ?? "", request.User.Id);
                        await _messageService.EditTemplateAsync(chatId.Value, request.AdminMessageId.Value, TelegramTemplate.Create(updatedText), ct);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to edit admin notification for request {RequestId}", request.Id);
                }
            }
            var replyMessage = await _localizer.GetInterfaceTranslation(Notifications.SupportRequestResolved, request.User.LanguageCode);
            await _messageService.SendTemplateAsync(request.UserId, TelegramTemplate.Create(replyMessage), ct);
        }
    }
}
