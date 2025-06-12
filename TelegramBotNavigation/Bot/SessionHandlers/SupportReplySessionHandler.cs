using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services.Sessions;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;

namespace TelegramBotNavigation.Bot.SessionHandlers
{
    public class SupportReplySessionHandler : ISessionHandler
    {
        public string Action => SessionKeys.SupportReply;

        private readonly ISessionManager _sessionManager;
        private readonly ISupportRequestService _supportService;
        private readonly ITelegramMessageService _messageService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<SupportReplySessionHandler> _logger;
        private readonly ILocalizationManager _localizer;

        public SupportReplySessionHandler(
            ISessionManager sessionManager,
            ISupportRequestService supportService,
            ITelegramMessageService messageService,
            IUserRepository userRepository,
            ILogger<SupportReplySessionHandler> logger,
            ILocalizationManager localizer)
        {
            _sessionManager = sessionManager;
            _supportService = supportService;
            _messageService = messageService;
            _userRepository = userRepository;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task HandleAsync(Message message, SessionData session, CancellationToken ct)
        {
            var adminId = message.From!.Id;
            var chatId = message.Chat.Id;
            var text = message.Text?.Trim();

            if (string.IsNullOrEmpty(text)) return;

            var admin = await _userRepository.GetByIdAsync(adminId);
            if (admin == null) return;

            if (!session.Data.TryGetValue("userId", out var userIdStr) || !long.TryParse(userIdStr, out var userId))
            {
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.SessionDataMissing, admin.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(errorMessage), ct);
                return;
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.UserNotFound, admin.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(errorMessage), ct);
                return;
            }

            var request = await _supportService.GetLastOpenRequestForUserAsync(userId);
            if (request == null)
            {
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.RequestNotFound, admin.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(errorMessage), ct);
                return;
            }

            await _supportService.AddMessageAsync(request.Id, isFromAdmin: true, text);

            var replyText = await _localizer.GetInterfaceTranslation(Messages.SupportReplyMessage, user.LanguageCode, text);
            await _messageService.SendTemplateAsync(userId, TelegramTemplate.Create(replyText), ct);

            var confirm = await _localizer.GetInterfaceTranslation(Notifications.SupportRequestReplySent, admin.LanguageCode);
            await _messageService.SendTemplateAsync(adminId, TelegramTemplate.Create(confirm), ct);

            await _sessionManager.SetSessionAsync(adminId, session, TimeSpan.FromMinutes(10));
        }
    }
}
