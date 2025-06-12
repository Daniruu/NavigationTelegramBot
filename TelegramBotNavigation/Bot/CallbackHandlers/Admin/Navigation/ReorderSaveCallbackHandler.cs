using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services.Sessions;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Utils;
using TelegramBotNavigation.Services;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Navigation
{
    public class ReorderSaveCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.ReorderSave;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ILogger<ReorderSelectItemCallbackHandler> _logger;
        private readonly ILocalizationManager _localizer;
        private readonly ITelegramMessageService _messageService;
        private readonly IReorderSessionManager _reorderSessionManager;
        private readonly ILanguageSettingRepository _languageSettingRepository;
        private readonly IMenuRepository _menuRepository;
        private readonly INavigationMessageService _navigationMessageService;
        private readonly ICallbackAlertService _callbackAlertService;

        public ReorderSaveCallbackHandler(
            IUserRepository userRepository,
            IUserService userService,
            ILogger<ReorderSelectItemCallbackHandler> logger,
            ILocalizationManager localizer, ITelegramMessageService messageService,
            IReorderSessionManager reorderSessionManager, 
            ILanguageSettingRepository languageSettingRepository,
            IMenuRepository menuRepository, 
            INavigationMessageService navigationMessageService,
            ICallbackAlertService callbackAlertService)
        {
            _userRepository = userRepository;
            _userService = userService;
            _logger = logger;
            _localizer = localizer;
            _messageService = messageService;
            _reorderSessionManager = reorderSessionManager;
            _languageSettingRepository = languageSettingRepository;
            _menuRepository = menuRepository;
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

            var session = await _reorderSessionManager.GetAsync(userId);
            if (session == null)
            {
                _logger.LogWarning("No active reorder session found.");
                var errorMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.SessionDataMissing, user.LanguageCode);
                await _callbackAlertService.ShowAsync(query.Id, errorMessage, cancellationToken: ct);
                return;
            }

            var menuId = session.MenuId;
            var menu = await _menuRepository.GetByIdAsync(menuId);
            if (menu == null)
            {
                var errorMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.MenuNotFound, user.LanguageCode);
                await _callbackAlertService.ShowAsync(query.Id, errorMessage, cancellationToken: ct);
                return;
            }


            foreach (var item in menu.MenuItems)
            {
                var snapshotItem = session.Items.FirstOrDefault(i => i.Id == item.Id);
                if (snapshotItem != null)
                {
                    item.Row = snapshotItem.Row;
                    item.Order = snapshotItem.Order;
                }
            }

            await _menuRepository.UpdateAsync(menu);
            await _navigationMessageService.UpdateAllNavigationMessagesAsync(ct);
            await _reorderSessionManager.ClearAsync(userId);

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

            var template = await NavigationEditTemplate.CreateAsync(user.LanguageCode, languageCode, _localizer, _languageSettingRepository, menu);
            await _messageService.EditTemplateAsync(chatId, messageId, template, ct);
        }
    }
}
