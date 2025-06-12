using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services.Sessions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;

namespace TelegramBotNavigation.Bot.SessionHandlers
{
    public class SupportRequestSessionHandler : ISessionHandler
    {
        public string Action => SessionKeys.SupportRequest;

        private readonly ISupportRequestService _supportService;
        private readonly ISessionManager _sessionManager;
        private readonly IUserRepository _userRepository;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly ITelegramClient _telegramClient;
        private readonly IBotSettingsService _settingsService;
        private readonly ILogger<SupportRequestSessionHandler> _logger;
        private readonly IUserInteractionService _userInteractionService;

        public SupportRequestSessionHandler(
            ISupportRequestService supportService,
            ISessionManager sessionManager,
            IUserRepository userRepository,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            ITelegramClient telegramClient,
            ILogger<SupportRequestSessionHandler> logger,
            IBotSettingsService settingsService,
            IUserInteractionService userInteractionService)
        {
            _supportService = supportService;
            _sessionManager = sessionManager;
            _userRepository = userRepository;
            _messageService = messageService;
            _localizer = localizer;
            _telegramClient = telegramClient;
            _logger = logger;
            _settingsService = settingsService;
            _userInteractionService = userInteractionService;
        }

        public async Task HandleAsync(Message message, SessionData session, CancellationToken ct)
        {
            var userId = message.From!.Id;
            var chatId = message.Chat.Id;
            var text = message.Text?.Trim();

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            if (string.IsNullOrWhiteSpace(text))
            {
                var error = await _localizer.GetInterfaceTranslation(Errors.InvalidInput, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(error), ct);
                return;
            }

            var supportGroupId = await _settingsService.GetSupportGroupIdAsync();
            if (supportGroupId == null)
            {
                _logger.LogWarning("SupportGroupId is not configured.");
                return;
            }

            var request = await _supportService.GetLastOpenRequestForUserAsync(userId);
            if (request == null)
            {
                request = await _supportService.CreateRequestAsync(userId, text);

                try
                {
                    var topicTitle = $"{user.FirstName}";
                    if (!string.IsNullOrEmpty(user.LastName)) topicTitle += $" {user.LastName}";

                    var topicId = await _telegramClient.CreateForumTopicAsync(supportGroupId.Value, topicTitle, emoji: "❗️", ct);
                    await _supportService.SetTelegramTopicAsync(request.Id, topicId);

                    var requestManageMarkup = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(await _localizer.GetInterfaceTranslation(Labels.CloseRequest, user.LanguageCode), $"{CallbackKeys.SupportResolve}:{request.Id}")
                        }
                    });

                    var managementMessage = await _messageService.SendTemplateInTopicAsync(
                        supportGroupId.Value, 
                        topicId, 
                        TelegramTemplate.Create(await _localizer.GetInterfaceTranslation(Headers.SupportRequestManage, LanguageCode.Ru), requestManageMarkup),
                        ct);

                    try
                    {
                        await _telegramClient.PinMessageAsync(
                            chatId: supportGroupId.Value,
                            messageId: managementMessage.MessageId,
                            disableNotification: true,
                            ct: ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to pin support management message for topic {TopicId}", topicId);
                    }

                    await _messageService.SendTemplateInTopicAsync(supportGroupId.Value, topicId, TelegramTemplate.Create(text), ct);

                    var chatIdNumeric = supportGroupId.Value.ToString().Replace("-100", "");
                    var topicUrl = $"https://t.me/c/{chatIdNumeric}/{topicId}";

                    var notifyText = await _localizer.GetInterfaceTranslation(Notifications.SupportRequestReceived, Enums.LanguageCode.Ru, user.FirstName, user.LastName ?? "", user.Id, text);

                    var markup = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithUrl($"✉️ {await _localizer.GetInterfaceTranslation(Labels.Reply, Enums.LanguageCode.Ru)}", topicUrl)
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(await _localizer.GetInterfaceTranslation(Labels.CloseRequest, Enums.LanguageCode.Ru), $"{CallbackKeys.SupportResolve}:{request.Id}")
                        }
                    });

                    var notifyMessage =await _messageService.SendTemplateAsync(supportGroupId.Value, TelegramTemplate.Create(notifyText, markup), ct);

                    await _supportService.SetAdminMessageIdAsync(request.Id, notifyMessage.MessageId);

                    await _userInteractionService.LogAsync(user, chatId, ActionType.SupportRequest, "");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error when creating a topic for a support request.");
                }
            }
            else
            {
                await _supportService.AddMessageAsync(request.Id, isFromAdmin: false, text);

                if (request.TopicId.HasValue)
                {
                    try
                    {
                        await _messageService.SendTemplateInTopicAsync(supportGroupId.Value, request.TopicId.Value, TelegramTemplate.Create(text), ct);
                    }
                    catch
                    {
                        await _sessionManager.ClearSessionAsync(userId);

                        await _supportService.CloseRequestAsync(request.Id);

                        var errorMessage = await _localizer.GetInterfaceTranslation(Errors.RequestAlreadyResolved, user.LanguageCode);
                        await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(errorMessage), ct);
                    }
                }
                else
                {
                    _logger.LogWarning("The support request {RequestId} does not have a TopicId.", request.Id);
                }
            }

            await _sessionManager.SetSessionAsync(userId, session);
        }
    }
}
