using System;
using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services.Sessions;
using TelegramBotNavigation.Utils;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;

namespace TelegramBotNavigation.Bot.SessionHandlers
{
    public class ItemAddSubmenuSessionHandler : ISessionHandler
    {
        public string Action => SessionKeys.NavigationItemAddSubmenu;

        private readonly IUserRepository _userRepository;
        private readonly ISessionManager _sessionManager;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly IMenuRepository _menuRepository;
        private readonly ILogger<ItemAddSubmenuSessionHandler> _logger;
        private readonly ITranslationService _translationService;
        private readonly ITranslationImageService _translationImageService;
        private readonly INavigationMessageService _navigationMessageService;
        private readonly ILanguageSettingRepository _languageSettingRepository;

        public ItemAddSubmenuSessionHandler(
            IUserRepository userRepository,
            ISessionManager sessionManager,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            IMenuRepository menuRepository,
            ILogger<ItemAddSubmenuSessionHandler> logger,
            ITranslationService translationService,
            ITranslationImageService translationImageService,
            INavigationMessageService navigationMessageService,
            ILanguageSettingRepository languageSettingRepository)
        {
            _userRepository = userRepository;
            _sessionManager = sessionManager;
            _messageService = messageService;
            _localizer = localizer;
            _menuRepository = menuRepository;
            _logger = logger;
            _translationService = translationService;
            _translationImageService = translationImageService;
            _navigationMessageService = navigationMessageService;
            _languageSettingRepository = languageSettingRepository;
        }

        public async Task HandleAsync(Message message, SessionData session, CancellationToken ct)
        {
            var userId = message.From!.Id;
            var chatId = message.Chat.Id;
            var title = message.Text;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            if (string.IsNullOrWhiteSpace(title))
            {
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.InvalidInput, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(errorMessage), ct);
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

            if (actionType != MenuActionType.SubMenu)
            {
                _logger.LogError("Invalid action type for UserId: {UserId}. Expected SubMenu, got {ActionType}", userId, actionType);
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

            var subMenu = new Menu
            {
                Title = $"{menu.Title}/{title}",
                IsMainMenu = false,
                HeaderTranslationKey = $"menu.header.{Guid.NewGuid()}",
                HeaderImageTranslationKey = $"menu.header.image.{Guid.NewGuid()}",
                CreatedAt = DateTime.UtcNow,
                ParentMenuId = menu.Id,
            };

            var languageCodes = await _languageSettingRepository.GetFallbackOrderAsync();
            foreach (var lang in languageCodes)
            {
                var parentHeader = await _translationService.GetTranslationAsync(menu.HeaderTranslationKey, lang);
                if (!string.IsNullOrEmpty(parentHeader))
                {
                    await _translationService.SetTranslationAsync(subMenu.HeaderTranslationKey, lang, parentHeader);
                }

                var parentImage = await _translationImageService.GetTranslationImageAsync(menu.HeaderImageTranslationKey, lang);
                if (!string.IsNullOrEmpty(parentImage))
                {
                    await _translationImageService.SetTranslationImageAsync(subMenu.HeaderImageTranslationKey, lang, parentImage);
                }
            }

            await _menuRepository.AddAsync(subMenu);

            var lastRow = menu.MenuItems.Any()
                ? menu.MenuItems.Max(i => i.Row)
                : -1;

            var newItem = new MenuItem
            {
                MenuId = menu.Id,
                LabelTranslationKey = $"menuitem.{Guid.NewGuid()}",
                ActionType = actionType,
                SubMenu = subMenu,
                Row = lastRow + 1,
                Order = 0,
            };

            menu.MenuItems.Add(newItem);

            await _menuRepository.SaveChangesAsync();

            await _translationService.SetTranslationAsync(newItem.LabelTranslationKey, LanguageCodeHelper.FromTelegramTag(langStr), label);

            await _navigationMessageService.UpdateAllNavigationMessagesAsync(ct);

            await _sessionManager.ClearSessionAsync(userId);

            var successMessage = await _localizer.GetInterfaceTranslation(Notifications.MenuItemAddSuccess, user.LanguageCode);
            await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(successMessage), ct);
            var languageCode = LanguageCodeHelper.FromTelegramTag(langStr);

            var template = await NavigationEditTemplate.CreateAsync(user.LanguageCode, languageCode, _localizer, _languageSettingRepository, menu);
            await _messageService.SendTemplateAsync(chatId, template, ct);
        }
    }
}
