using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Services.Interfaces;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;
using TelegramBotNavigation.Services;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Sessions;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Bot.MessageHandlers
{
    public class AdminReplyMessageHandler : IMessageHandler
    {
        private readonly ISupportRequestService _supportService;
        private readonly ITelegramMessageService _messageService;
        private readonly ILogger<AdminReplyMessageHandler> _logger;
        private readonly ISessionManager _sessionManager;
        private readonly ILocalizationManager _localizer;

        public AdminReplyMessageHandler(
            ISupportRequestService supportService,
            ITelegramMessageService messageService,
            ILogger<AdminReplyMessageHandler> logger,
            ISessionManager sessionManager,
            ILocalizationManager localizer)
        {
            _supportService = supportService;
            _messageService = messageService;
            _logger = logger;
            _sessionManager = sessionManager;
            _localizer = localizer;
        }

        public bool CanHandle(Message message)
        {
            return message.Chat.Type == ChatType.Supergroup && message.MessageThreadId.HasValue;
        }

        public async Task HandleAsync(Message message, CancellationToken ct)
        {
            var topicId = message.MessageThreadId!.Value;
            var user = message.From;
            var chatId = message.Chat.Id;

            if (user == null) return;

            var request = await _supportService.GetRequestByTopicIdAsync(topicId);
            if (request == null)
            {
                _logger.LogWarning("No support request found for topic {TopicId}", topicId);
                return;
            }

            var text = message.Text?.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            var targetUserSession = await _sessionManager.GetSessionAsync(request.UserId);
            bool userSessionClosed = targetUserSession == null || targetUserSession.Action != SessionKeys.SupportRequest;
            if (userSessionClosed)
            {
                var adminWarning = await _localizer.GetInterfaceTranslation(Notifications.RequestUserSessionClosed, LanguageCodeHelper.FromTelegramTag(user.LanguageCode));
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(adminWarning), ct);

                var notifyUser = await _localizer.GetInterfaceTranslation(Notifications.UserSessionClosedPrompt, request.User.LanguageCode);
                await _messageService.SendTemplateAsync(request.UserId, TelegramTemplate.Create(notifyUser), ct);
            }

            _logger.LogInformation("Forwarding admin reply from topic {TopicId} to user {UserId}", topicId, request.UserId);

            var replyMessage = $"<b>{user.FirstName}</b>\n{text}";

            await _supportService.AddMessageAsync(request.Id, isFromAdmin: true, text);
            await _messageService.SendTemplateAsync(request.UserId, TelegramTemplate.Create(replyMessage), ct);
        }
    }
}
