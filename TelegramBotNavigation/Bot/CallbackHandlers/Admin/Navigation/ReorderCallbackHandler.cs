using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Repositories;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services.Sessions;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Navigation
{
    public class ReorderCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.NavigationReorder;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ILogger<ReorderCallbackHandler> _logger;
        private readonly ILocalizationManager _localizer;
        private readonly ITelegramMessageService _messageService;
        private readonly ISessionManager _sessionManager;
        private readonly IMenuRepository _menuRepository;
        private readonly ILanguageSettingRepository _languageSettingRepository;
        private readonly IReorderSessionManager _reorderSessionService;

        public ReorderCallbackHandler(
            IUserRepository userRepository,
            IUserService userService, 
            ILogger<ReorderCallbackHandler> logger, 
            ILocalizationManager localizer, 
            ITelegramMessageService messageService, 
            ISessionManager sessionManager, 
            IMenuRepository menuRepository, 
            ILanguageSettingRepository languageSettingRepository, 
            IReorderSessionManager reorderSessionService)
        {
            _userRepository = userRepository;
            _userService = userService;
            _logger = logger;
            _localizer = localizer;
            _messageService = messageService;
            _sessionManager = sessionManager;
            _menuRepository = menuRepository;
            _languageSettingRepository = languageSettingRepository;
            _reorderSessionService = reorderSessionService;
        }

        public async Task HandleAsync(CallbackQuery query, string[] args, CancellationToken ct)
        {
            var chatId = query.Message!.Chat.Id;
            var messageId = query.Message.MessageId;
            var userId = query.From.Id;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            if (!_userService.IsAdmin(user))
            {
                _logger.LogWarning("User {UserId} not found when trying to access admin command.", userId);
                var errorMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.NotAdmin, user.LanguageCode);
                var errorTemplate = TelegramTemplate.Create(errorMessage);
                await _messageService.SendTemplateAsync(chatId, errorTemplate, ct);
                return;
            }

            await _sessionManager.ClearSessionAsync(userId);

            if (args.Length == 0) return;

            var menuIdStr = args[0];
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

            var snapshotItems = new List<MenuItemSnapshot>();

            foreach (var item in menu.MenuItems)
            {
                var label = await _localizer.GetCustomTranslationAsync(item.LabelTranslationKey, user.LanguageCode) ?? "[No Label]";
                snapshotItems.Add(new MenuItemSnapshot
                {
                    Id = item.Id,
                    Label = label,
                    Row = item.Row,
                    Order = item.Order
                });
            }

            var session = new MenuReorderSession
            {
                MenuId = menuId,
                SelectedItemId = snapshotItems.FirstOrDefault()?.Id ?? 0,
                Items = snapshotItems
            };

            await _reorderSessionService.StartAsync(userId, session);

            var template = await NavigationReorderTemplate.CreateAsync(user.LanguageCode, _localizer, session);
            await _messageService.EditTemplateAsync(chatId, messageId, template, ct);
        }
    }
}
