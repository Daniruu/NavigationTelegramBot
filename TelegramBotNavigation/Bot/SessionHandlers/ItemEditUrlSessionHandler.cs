using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;
using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services.Sessions;
using TelegramBotNavigation.Utils;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Services;

namespace TelegramBotNavigation.Bot.SessionHandlers
{
    public class ItemEditUrlSessionHandler : ISessionHandler
    {
        public string Action => SessionKeys.NavigationItemEditUrl;

        private readonly IUserRepository _userRepository;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly ISessionManager _sessionManager;
        private readonly ILogger<ItemEditUrlSessionHandler> _logger;
        private readonly ILanguageSettingRepository _languageSettingRepository;
        private readonly IMenuRepository _menuRepository;
        private readonly INavigationMessageService _navigationMessageService;

        public ItemEditUrlSessionHandler(
            IUserRepository userRepository,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            ISessionManager sessionManager,
            ILogger<ItemEditUrlSessionHandler> logger,
            ILanguageSettingRepository languageSettingRepository,
            IMenuRepository menuRepository,
            INavigationMessageService navigationMessageService)
        {
            _userRepository = userRepository;
            _messageService = messageService;
            _localizer = localizer;
            _sessionManager = sessionManager;
            _logger = logger;
            _languageSettingRepository = languageSettingRepository;
            _menuRepository = menuRepository;
            _navigationMessageService = navigationMessageService;
        }

        public async Task HandleAsync(Message message, SessionData session, CancellationToken ct)
        {
            var userId = message.From!.Id;
            var chatId = message.Chat.Id;
            var url = message.Text;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            if (string.IsNullOrWhiteSpace(url))
            {
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.InvalidInput, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(errorMessage), ct);
                return;
            }

            if (
                !session.Data.TryGetValue("menuId", out var menuIdStr) || 
                !session.Data.TryGetValue("menuItemId", out var menuItemIdStr) ||
                !session.Data.TryGetValue("lang", out var langStr))
            {
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.SessionDataMissing, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(errorMessage), ct);
                return;
            }

            if (!InputValidator.IsValidUrl(url))
            {
                var error = await _localizer.GetInterfaceTranslation(Errors.InvalidUrl, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(error), ct);
                return;
            }
            session.Data["url"] = url;

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

            if (!int.TryParse(menuItemIdStr, out var menuItemId))
            {
                _logger.LogWarning("Invalid menuItemId format: {menuItemIdStr}", menuItemIdStr);
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.InvalidMenuItemId, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(errorMessage), ct);
                return;
            }

            var itemToEdit = menu.MenuItems.FirstOrDefault(i => i.Id == menuItemId);
            if (itemToEdit == null) return;

            itemToEdit.Url = url;

            await _menuRepository.UpdateAsync(menu);
            await _navigationMessageService.UpdateAllNavigationMessagesAsync(ct);

            var languageCode = LanguageCodeHelper.FromTelegramTag(langStr);

            await _sessionManager.ClearSessionAsync(userId);

            var successMessage = await _localizer.GetInterfaceTranslation(Notifications.MenuItemEditSuccess, user.LanguageCode);
            await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(successMessage), ct);

            var template = await NavigationEditTemplate.CreateAsync(user.LanguageCode, languageCode, _localizer, _languageSettingRepository, menu);
            await _messageService.SendTemplateAsync(chatId, template, ct);
        }
    }
}
