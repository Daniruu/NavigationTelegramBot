using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services.Sessions;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Bot.SessionHandlers
{
    public class ItemAddUrlSessionHandler : ISessionHandler
    {
        public string Action => SessionKeys.NavigationItemAddUrl;

        private readonly IUserRepository _userRepository;
        private readonly IMenuRepository _menuRepository;
        private readonly ISessionManager _sessionManager;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly ILanguageSettingRepository _languageSettingRepository;
        private readonly ILogger<ItemAddUrlSessionHandler> _logger;
        private readonly ITranslationService _translationService;
        private readonly INavigationMessageService _navigationMessageService;

        public ItemAddUrlSessionHandler(
            IUserRepository userRepository,
            IMenuRepository menuRepository,
            ISessionManager sessionManager,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            ILanguageSettingRepository languageSettingRepository,
            ILogger<ItemAddUrlSessionHandler> logger,
            ITranslationService translationService,
            INavigationMessageService navigationMessageService)
        {
            _userRepository = userRepository;
            _menuRepository = menuRepository;
            _sessionManager = sessionManager;
            _messageService = messageService;
            _localizer = localizer;
            _languageSettingRepository = languageSettingRepository;
            _logger = logger;
            _translationService = translationService;
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
                var error = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.InvalidInput, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(error), ct);
                return;
            }

            if (!InputValidator.IsValidUrl(url))
            {
                var error = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.InvalidUrl, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(error), ct);
                return;
            }

            if (!session.Data.TryGetValue("menuId", out var menuIdStr) ||
                !session.Data.TryGetValue("type", out var typeStr) ||
                !session.Data.TryGetValue("lang", out var langStr) ||
                !session.Data.TryGetValue("label", out var label))
            {
                _logger.LogError("Session data missing for UserId: {UserId}. Session: {@SessionData}", userId, session.Data);
                var error = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.SessionDataMissing, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(error), ct);
                return;
            }

            if (!int.TryParse(menuIdStr, out var menuId) ||
                !Enum.TryParse<MenuActionType>(typeStr, out var actionType))
            {
                _logger.LogError("Invalid session data format for UserId: {UserId}. MenuIdStr: {MenuIdStr}, TypeStr: {TypeStr}", userId, menuIdStr, typeStr);
                var error = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.InvalidSessionData, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(error), ct);
                return;
            }

            if (actionType != MenuActionType.Url)
            {
                _logger.LogError("Invalid action type for UserId: {UserId}. Expected Url, got {ActionType}", userId, actionType);
                var error = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.InvalidActionType, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(error), ct);
                return;
            }

            var menu = await _menuRepository.GetByIdAsync(menuId);
            if (menu == null)
            {
                _logger.LogError("Menu with ID {MenuId} not found.", menuId);
                var error = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.MenuNotFound, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(error), ct);
                return;
            }

            var lastRow = menu.MenuItems.Any()
                ? menu.MenuItems.Max(i => i.Row)
                : -1;

            var newItem = new MenuItem
            {
                MenuId = menu.Id,
                LabelTranslationKey = $"menuitem.{Guid.NewGuid()}",
                ActionType = actionType,
                Url = url,
                Row = lastRow + 1,
                Order = 0,
            };

            _logger.LogInformation("Creating new menu item: Id: {ItemId}, LabelKey: {LabelKey}, ActionType: {ActionType}, Value: {Value}, Row: {Row}, Order: {Order}",
                newItem.Id, newItem.LabelTranslationKey, newItem.ActionType, newItem.Url, newItem.Row, newItem.Order);

            menu.MenuItems.Add(newItem);

            await _menuRepository.SaveChangesAsync();

            await _translationService.SetTranslationAsync(newItem.LabelTranslationKey, LanguageCodeHelper.FromTelegramTag(langStr), label);

            await _navigationMessageService.UpdateAllNavigationMessagesAsync(ct);

            await _sessionManager.ClearSessionAsync(userId);

            var successMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Notifications.MenuItemAddSuccess, user.LanguageCode);
            await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(successMessage), ct);

            var languageCode = LanguageCodeHelper.FromTelegramTag(langStr);

            var template = await NavigationEditTemplate.CreateAsync(user.LanguageCode, languageCode, _localizer, _languageSettingRepository, menu);
            await _messageService.SendTemplateAsync(chatId, template, ct);
        }
    }
}
