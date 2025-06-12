using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Utils;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Navigation
{
    public class NavigationSetDefaultCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.NavigationSetDefault;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ILogger<NavigationSetDefaultCallbackHandler> _logger;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly IMenuRepository _menuRepository;
        private readonly ILanguageSettingRepository _languageSettingRepository;
        private readonly IMenuButtonBuilder _buttonBuilder;
        private readonly INavigationMessageService _navigationMessageService;
        private readonly ICallbackAlertService _callbackAlertService;

        public NavigationSetDefaultCallbackHandler(
            IUserRepository userRepository, 
            IUserService userService, 
            ILogger<NavigationSetDefaultCallbackHandler> logger, 
            ITelegramMessageService messageService, 
            ILocalizationManager localizer, 
            IMenuRepository menuRepository, 
            ILanguageSettingRepository languageSettingRepository, 
            IMenuButtonBuilder buttonBuilder,
            INavigationMessageService navigationMessageService,
            ICallbackAlertService callbackAlertService)
        {
            _userRepository = userRepository;
            _userService = userService;
            _logger = logger;
            _messageService = messageService;
            _localizer = localizer;
            _menuRepository = menuRepository;
            _languageSettingRepository = languageSettingRepository;
            _buttonBuilder = buttonBuilder;
            _navigationMessageService = navigationMessageService;
            _callbackAlertService = callbackAlertService;
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
                await _callbackAlertService.ShowAsync(query.Id, errorMessage, cancellationToken: ct);
                return;
            }

            if (args.Length == 0) return;

            var menuIdStr = args[0];
            if (!int.TryParse(menuIdStr, out var menuId))
            {
                _logger.LogWarning("Invalid menuId format: {MenuIdStr}", menuIdStr);
                var errorMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.InvalidMenuId, user.LanguageCode);
                await _callbackAlertService.ShowAsync(query.Id, errorMessage, cancellationToken: ct);
                return;
            }

            var navigationMenus = await _menuRepository.GetTopLevelMenusAsync();

            var selectedMenu = navigationMenus.FirstOrDefault(m => m.Id == menuId);
            if (selectedMenu == null) return;

            foreach (var menu in navigationMenus)
            {
                menu.IsMainMenu = menu.Id == selectedMenu.Id;
                await _menuRepository.UpdateAsync(menu);
            }
            await _navigationMessageService.UpdateAllNavigationMessagesAsync(ct);

            LanguageCode languageCode;

            if (args.Length < 2)
            {
                var fallbackOrder = await _languageSettingRepository.GetFallbackOrderAsync();
                languageCode = fallbackOrder.First();
            }
            else
            {
                var language = args[1];
                languageCode = LanguageCodeHelper.FromTelegramTag(language);
            }

            var alert = await _localizer.GetInterfaceTranslation(LocalizationKeys.Notifications.NavigationSetDefaultSuccess, user.LanguageCode);
            await _callbackAlertService.ShowAsync(query.Id, alert, cancellationToken: ct);

            var template = await NavigationViewTemplate.CreateAsync(user.LanguageCode, languageCode, _localizer, _languageSettingRepository, _buttonBuilder, selectedMenu);
            await _messageService.EditTemplateAsync(chatId, messageId, template, ct);
        }
    }
}
