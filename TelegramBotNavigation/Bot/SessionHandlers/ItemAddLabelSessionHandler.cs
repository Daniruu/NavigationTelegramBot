using System;
using System.Reflection.Emit;
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
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;

namespace TelegramBotNavigation.Bot.SessionHandlers
{
    public class ItemAddLabelSessionHandler : ISessionHandler
    {
        public string Action => SessionKeys.NavigationItemAddLabel;

        private readonly IUserRepository _userRepository;
        private readonly ISessionManager _sessionManager;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly IMenuRepository _menuRepository;
        private readonly ILogger<ItemAddLabelSessionHandler> _logger;
        private readonly ITranslationService _translationService;
        private readonly INavigationMessageService _navigationMessageService;
        private readonly ILanguageSettingRepository _languageSettingRepository;

        public ItemAddLabelSessionHandler(
            IUserRepository userRepository,
            ISessionManager sessionManager,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            IMenuRepository menuRepository,
            ILogger<ItemAddLabelSessionHandler> logger,
            ITranslationService translationService,
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
            _navigationMessageService = navigationMessageService;
            _languageSettingRepository = languageSettingRepository;
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

            if (!session.Data.TryGetValue("menuId", out var menuIdStr) ||
                !session.Data.TryGetValue("type", out var actionTypeStr) ||
                !session.Data.TryGetValue("lang", out var langStr))
            {
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.SessionDataMissing, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(errorMessage), ct);
                return;
            }

            session.Data["label"] = text;

            if (!Enum.TryParse<MenuActionType>(actionTypeStr, out var type))
            {
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.InvalidSessionData, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(errorMessage), ct);
                return;
            }

            switch (type)
            {
                case MenuActionType.Url:
                    session.Action = SessionKeys.NavigationItemAddUrl;

                    var urlPrompt = await _localizer.GetInterfaceTranslation(Messages.EnterButtonUrl, user.LanguageCode);
                    await _sessionManager.SetSessionAsync(userId, session, TimeSpan.FromMinutes(10));
                    await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(urlPrompt), ct);
                    break;

                case MenuActionType.ShowMessage:
                    session.Action = SessionKeys.NavigationItemAddMessage;

                    var messagePrompt = await _localizer.GetInterfaceTranslation(Messages.EnterButtonMessage, user.LanguageCode);
                    await _sessionManager.SetSessionAsync(userId, session, TimeSpan.FromMinutes(10));
                    await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(messagePrompt), ct);
                    break;

                case MenuActionType.SubMenu:
                    session.Action = SessionKeys.NavigationItemAddSubmenu;
                    
                    var submenuPrompt = await _localizer.GetInterfaceTranslation(Messages.EnterMenuTitle, user.LanguageCode);
                    await _sessionManager.SetSessionAsync(userId, session, TimeSpan.FromMinutes(10));
                    await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(submenuPrompt), ct);

                    break;

                case MenuActionType.SupportRequest:
                    if (!int.TryParse(menuIdStr, out var menuId))
                    {
                        _logger.LogError("Invalid session data format for UserId: {UserId}. MenuIdStr: {MenuIdStr}", userId, menuIdStr);
                        var error = await _localizer.GetInterfaceTranslation(Errors.InvalidSessionData, user.LanguageCode);
                        await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(error), ct);
                        return;
                    }

                    var menu = await _menuRepository.GetByIdAsync(menuId);
                    if (menu == null)
                    {
                        _logger.LogError("Menu with ID {MenuId} not found.", menuId);
                        var error = await _localizer.GetInterfaceTranslation(Errors.MenuNotFound, user.LanguageCode);
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
                        ActionType = type,
                        Row = lastRow + 1,
                        Order = 0,
                    };
                    menu.MenuItems.Add(newItem);
                    await _menuRepository.SaveChangesAsync();

                    var languageCode = LanguageCodeHelper.FromTelegramTag(langStr);

                    await _translationService.SetTranslationAsync(newItem.LabelTranslationKey, languageCode, text);

                    await _navigationMessageService.UpdateAllNavigationMessagesAsync(ct);

                    await _sessionManager.ClearSessionAsync(userId);

                    var successMessage = await _localizer.GetInterfaceTranslation(Notifications.MenuItemAddSuccess, user.LanguageCode);
                    await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(successMessage), ct);

                    var template = await NavigationEditTemplate.CreateAsync(user.LanguageCode, languageCode, _localizer, _languageSettingRepository, menu);
                    await _messageService.SendTemplateAsync(chatId, template, ct);

                    break;

                default:
                    await _sessionManager.ClearSessionAsync(userId);

                    var warning = await _localizer.GetInterfaceTranslation(Messages.NotImplementedYet, user.LanguageCode);
                    await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(warning), ct);
                    break;
            }
        }
    }
}
