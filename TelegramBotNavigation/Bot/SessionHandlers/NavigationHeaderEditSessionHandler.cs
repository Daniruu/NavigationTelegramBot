using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Services.Sessions;
using TelegramBotNavigation.Utils;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;

namespace TelegramBotNavigation.Bot.SessionHandlers
{
    public class NavigationHeaderEditSessionHandler : ISessionHandler
    {
        public string Action => SessionKeys.NavigationHeaderEdit;

        private readonly IUserRepository _userRepository;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly ILogger<NavigationHeaderEditSessionHandler> _logger;
        private readonly ISessionManager _sessionManager;
        private readonly IMenuRepository _menuRepository;
        private readonly ITranslationService _translationService;
        private readonly ILanguageSettingRepository _languageSettingRepository;

        public NavigationHeaderEditSessionHandler(
            IUserRepository userRepository, 
            ITelegramMessageService messageService, 
            ILocalizationManager localizer, 
            ILogger<NavigationHeaderEditSessionHandler> logger, 
            ISessionManager sessionManager, 
            IMenuRepository menuRepository, 
            ITranslationService translationService, 
            ILanguageSettingRepository languageSettingRepository)
        {
            _userRepository = userRepository;
            _messageService = messageService;
            _localizer = localizer;
            _logger = logger;
            _sessionManager = sessionManager;
            _menuRepository = menuRepository;
            _translationService = translationService;
            _languageSettingRepository = languageSettingRepository;
        }

        public async Task HandleAsync(Message message, SessionData session, CancellationToken ct)
        {
            var userId = message.From!.Id;
            var chatId = message.Chat.Id;
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            if (!session.Data.TryGetValue("menuId", out var menuIdStr) ||
                !session.Data.TryGetValue("lang", out var langTag))
            {
                var error = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.SessionDataMissing, user.LanguageCode);
                var errorTemplate = TelegramTemplate.Create(error);
                await _messageService.SendTemplateAsync(chatId, errorTemplate, ct);
                return;
            }

            if (!int.TryParse(menuIdStr, out var menuId))
            {
                _logger.LogWarning("Invalid menuId format: {MenuIdStr}", menuIdStr);
                var errorMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.InvalidMenuId, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(errorMessage), ct);
                return;
            }

            var menu = await _menuRepository.GetByIdAsync(menuId);
            if (menu == null)
            {
                var error = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.MenuNotFound, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(error), ct);
                return;
            }

            var languageCode = LanguageCodeHelper.FromTelegramTag(langTag);

            string? header = message.Text;

            if (string.IsNullOrWhiteSpace(header))
            {
                var error = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.InvalidInput, user.LanguageCode);
                var errorTemplate = TelegramTemplate.Create(error);
                await _messageService.SendTemplateAsync(chatId, errorTemplate, ct);
                return;
            }

            await _translationService.SetTranslationAsync(menu.HeaderTranslationKey, languageCode, header);

            await _sessionManager.ClearSessionAsync(userId);

            var successMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Notifications.NavigationHeaderEditSuccess, user.LanguageCode);
            await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(successMessage), ct);

            var template = await NavigationEditTemplate.CreateAsync(user.LanguageCode, languageCode, _localizer, _languageSettingRepository, menu);
            await _messageService.SendTemplateAsync(chatId, template, ct);
        }
    }
}
