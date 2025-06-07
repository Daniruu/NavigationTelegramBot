using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Services.Sessions;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Utils;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories;

namespace TelegramBotNavigation.Bot.SessionHandlers
{
    public class ItemEditLabelSessionHandler : ISessionHandler
    {
        public string Action => SessionKeys.NavigationItemEditLabel;

        private readonly IUserRepository _userRepository;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly ISessionManager _sessionManager;
        private readonly ILogger<ItemEditLabelSessionHandler> _logger;
        private readonly ITranslationService _translationService;
        private readonly ILanguageSettingRepository _languageSettingRepository;
        private readonly IMenuRepository _menuRepository;

        public ItemEditLabelSessionHandler(
            IUserRepository userRepository,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            ISessionManager sessionManager,
            ILogger<ItemEditLabelSessionHandler> logger,
            ITranslationService translationService,
            ILanguageSettingRepository languageSettingRepository,
            IMenuRepository menuRepository)
        {
            _userRepository = userRepository;
            _messageService = messageService;
            _localizer = localizer;
            _sessionManager = sessionManager;
            _logger = logger;
            _translationService = translationService;
            _languageSettingRepository = languageSettingRepository;
            _menuRepository = menuRepository;
        }

        public async Task HandleAsync(Message message, SessionData session, CancellationToken ct)
        {
            var userId = message.From!.Id;
            var chatId = message.Chat.Id;
            var text = message.Text;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            if (string.IsNullOrWhiteSpace(text))
            {
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.InvalidInput, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(errorMessage), ct);
                return;
            }

            if (!session.Data.TryGetValue("translationKey", out var tranlationKey) || !session.Data.TryGetValue("lang", out var lang))
            {
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.SessionDataMissing, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(errorMessage), ct);
                return;
            }

            session.Data["label"] = text;

            var languageCode = LanguageCodeHelper.FromTelegramTag(lang);

            await _translationService.SetTranslationAsync(tranlationKey, languageCode, text);

            await _sessionManager.ClearSessionAsync(userId);

            var successMessage = await _localizer.GetInterfaceTranslation(Notifications.MenuItemEditSuccess, user.LanguageCode);
            await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(successMessage), ct);

            var menuIdStr = session.Data.GetValueOrDefault("menuId");
            if (!int.TryParse(menuIdStr, out var menuId))
            {
                _logger.LogError("Menu ID is missing or invalid in session data for UserId: {UserId}. Session: {@SessionData}", userId, session.Data);
                return;
            }

            var menu = await _menuRepository.GetByIdAsync(menuId);
            if (menu == null)
            {
                var error = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.MenuNotFound, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(error), ct);
                return;
            }

            var template = await NavigationEditTemplate.CreateAsync(user.LanguageCode, languageCode, _localizer, _languageSettingRepository, menu);
            await _messageService.SendTemplateAsync(chatId, template, ct);
        }
    }
}
