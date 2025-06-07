using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Utils;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using Telegram.Bot;

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

        public ItemDeleteCallbackHandler(
            IUserRepository userRepository,
            IUserService userService,
            ILogger<NavigationViewCallbackHandler> logger,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            IMenuRepository menuRepository,
            ILanguageSettingRepository languageSettingRepository,
            INavigationMessageService navigationMessageService,
            ITranslationRepository translationRepository)
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

            if (args.Length < 2) return;

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

            var menuItemIdStr = args[1];
            if (!int.TryParse(menuItemIdStr, out var menuItemId))
            {
                _logger.LogWarning("Invalid menuItemId format: {menuItemIdStr}", menuItemIdStr);
                var errorMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.InvalidMenuItemId, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(errorMessage), ct);
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

            var successMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Notifications.MenuItemDeleteSuccess, user.LanguageCode);
            await _messageService.EditTemplateAsync(chatId, messageId, TelegramTemplate.Create(successMessage), ct);

            var template = await ItemDeleteOptionsTemplate.CreateAsync(user.LanguageCode, languageCode, _localizer, _languageSettingRepository, menu);
            await _messageService.SendTemplateAsync(chatId, template, ct);
        }
    }
}
