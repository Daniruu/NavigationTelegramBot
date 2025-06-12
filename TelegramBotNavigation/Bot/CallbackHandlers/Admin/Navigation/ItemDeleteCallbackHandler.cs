using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Utils;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using Telegram.Bot;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;
using TelegramBotNavigation.Services;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Navigation
{
    public class ItemDeleteCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.ItemDelete;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ILogger<NavigationViewCallbackHandler> _logger;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly IMenuRepository _menuRepository;
        private readonly ILanguageSettingRepository _languageSettingRepository;
        private readonly INavigationMessageService _navigationMessageService;
        private readonly ITranslationRepository _translationRepository;
        private readonly ICallbackAlertService _callbackAlertService;

        public ItemDeleteCallbackHandler(
            IUserRepository userRepository,
            IUserService userService,
            ILogger<NavigationViewCallbackHandler> logger,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            IMenuRepository menuRepository,
            ILanguageSettingRepository languageSettingRepository,
            INavigationMessageService navigationMessageService,
            ITranslationRepository translationRepository,
            ICallbackAlertService callbackAlertService)
        {
            _userRepository = userRepository;
            _userService = userService;
            _logger = logger;
            _messageService = messageService;
            _localizer = localizer;
            _menuRepository = menuRepository;
            _languageSettingRepository = languageSettingRepository;
            _navigationMessageService = navigationMessageService;
            _translationRepository = translationRepository;
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
                _logger.LogWarning("Access denied for user {UserId} while trying to access admin callback.", userId);
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.NotAdmin, user.LanguageCode);
                await _callbackAlertService.ShowAsync(query.Id, errorMessage, showAlert: true, cancellationToken: ct);
                return;
            }

            if (args.Length < 2) return;

            var menuIdStr = args[0];
            if (!int.TryParse(menuIdStr, out var menuId))
            {
                _logger.LogWarning("Invalid menuId format: {MenuIdStr}", menuIdStr);
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.InvalidMenuId, user.LanguageCode);
                await _callbackAlertService.ShowAsync(query.Id, errorMessage, cancellationToken: ct);
                return;
            }

            var menu = await _menuRepository.GetByIdAsync(menuId);
            if (menu == null)
            {
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.MenuNotFound, user.LanguageCode);
                await _callbackAlertService.ShowAsync(query.Id, errorMessage, cancellationToken: ct);
                return;
            }

            var menuItemIdStr = args[1];
            if (!int.TryParse(menuItemIdStr, out var menuItemId))
            {
                _logger.LogWarning("Invalid menuItemId format: {menuItemIdStr}", menuItemIdStr);
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.InvalidMenuItemId, user.LanguageCode);
                await _callbackAlertService.ShowAsync(query.Id, errorMessage, cancellationToken: ct);
                return;
            }

            var itemToRemove = menu.MenuItems.FirstOrDefault(i => i.Id == menuItemId);
            if (itemToRemove == null) return;

            var labelKey = itemToRemove.LabelTranslationKey;
            var messageKey = itemToRemove.MessageTranslationKey;

            await _translationRepository.DeleteAllByKeyAsync(labelKey);
            if (!string.IsNullOrWhiteSpace(messageKey))
            {
                await _translationRepository.DeleteAllByKeyAsync(messageKey);
            }

            menu.MenuItems.Remove(itemToRemove);

            var groupedByRow = menu.MenuItems.GroupBy(i => i.Row);
            foreach ( var group in groupedByRow)
            {
                var ordered = group.OrderBy(i => i.Order).ToList();
                for (int i = 0; i < ordered.Count; i++)
                {
                    ordered[i].Order = i;
                }
            }
            
            var usedRow = menu.MenuItems.Select(i => i.Row).Distinct().OrderBy(r => r).ToList();

            for (int newRowIndex = 0; newRowIndex < usedRow.Count; newRowIndex++)
            {
                var oldRowIndex = usedRow[newRowIndex];
                var rowItems = menu.MenuItems.Where(i => i.Row == oldRowIndex).ToList();
                foreach (var item in rowItems)
                {
                    item.Row = oldRowIndex;
                }
            }

            await _menuRepository.UpdateAsync(menu);

            await _navigationMessageService.UpdateAllNavigationMessagesAsync(ct);

            LanguageCode languageCode;

            if (args.Length < 3)
            {
                var fallbackOrder = await _languageSettingRepository.GetFallbackOrderAsync();
                languageCode = fallbackOrder.First();
            }
            else
            {
                var language = args[2];
                languageCode = LanguageCodeHelper.FromTelegramTag(language);
            }

            var successMessage = await _localizer.GetInterfaceTranslation(Notifications.MenuItemDeleteSuccess, user.LanguageCode);
            await _callbackAlertService.ShowAsync(query.Id, successMessage, cancellationToken: ct);

            var template = await ItemDeleteOptionsTemplate.CreateAsync(user.LanguageCode, languageCode, _localizer, _languageSettingRepository, menu);
            await _messageService.EditTemplateAsync(chatId, messageId, template, ct);
        }
    }
}
