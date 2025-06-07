using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Repositories;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services.Sessions;
using TelegramBotNavigation.Utils;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;

namespace TelegramBotNavigation.Bot.SessionHandlers
{
    public class WelcomeEditSessionHandler : ISessionHandler
    {
        public string Action => SessionKeys.WelcomeEdit;

        private readonly IUserRepository _userRepository;
        private readonly IWelcomeMessageRepository _welcomeMessageRepository;
        private readonly ILocalizationManager _localizer;
        private readonly ISessionManager _sessionManager;
        private readonly ITelegramMessageService _messageService;
        private readonly ILogger<WelcomeEditSessionHandler> _logger;
        private readonly ILanguageSettingRepository _languageSettingRepository;
        private readonly IWelcomeMessageProvider _welcomeMessageProvider;

        public WelcomeEditSessionHandler(
            IUserRepository userRepository,
            IWelcomeMessageRepository welcomeMessageRepository,
            ILocalizationManager localizer,
            ISessionManager sessionManager,
            ITelegramMessageService messageService,
            ILogger<WelcomeEditSessionHandler> logger,
            ILanguageSettingRepository languageSettingRepository,
            IWelcomeMessageProvider welcomeMessageProvider)
        {
            _userRepository = userRepository;
            _welcomeMessageRepository = welcomeMessageRepository;
            _localizer = localizer;
            _sessionManager = sessionManager;
            _messageService = messageService;
            _logger = logger;
            _languageSettingRepository = languageSettingRepository;
            _welcomeMessageProvider = welcomeMessageProvider;
        }

        public async Task HandleAsync(Message message, SessionData session, CancellationToken ct)
        {
            var userId = message.From!.Id;
            var chatId = message.Chat.Id;
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            if (!session.Data.TryGetValue("lang", out var langTag))
            {
                var error = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.SessionDataMissing, user.LanguageCode);
                var errorTemplate = TelegramTemplate.Create(error);
                await _messageService.SendTemplateAsync(chatId, errorTemplate, ct);
                return;
            }

            var languageCode = LanguageCodeHelper.FromTelegramTag(langTag);

            string? text = message.Text ?? message.Caption;
            string? imageFileId = null;

            var photo = message.Photo?.OrderByDescending(p => p.FileSize).FirstOrDefault();
            if (photo != null)
            {
                imageFileId = photo.FileId;
            }
            else if (message.Document != null && message.Document.MimeType!.StartsWith("image/"))
            {
                imageFileId = message.Document.FileId;
            }

            if (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(imageFileId))
            {
                var error = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.InvalidInput, user.LanguageCode);
                var errorTemplate = TelegramTemplate.Create(error);
                await _messageService.SendTemplateAsync(chatId, errorTemplate, ct);
                return;
            }
            await _welcomeMessageRepository.SetAsync(languageCode, text, imageFileId);

            _logger.LogInformation($"Welcome message updated: {languageCode}, text: {text != null}, image: {imageFileId != null}");

            await _sessionManager.ClearSessionAsync(userId);

            var successMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Notifications.WelcomeEditSuccess, user.LanguageCode);
            await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(successMessage), ct);

            var template = await WelcomeManageTemplate.CreateAsync(user.LanguageCode, languageCode, _localizer, _languageSettingRepository, _welcomeMessageProvider);
            await _messageService.SendTemplateAsync(chatId, template, ct);
        }
    }
}
