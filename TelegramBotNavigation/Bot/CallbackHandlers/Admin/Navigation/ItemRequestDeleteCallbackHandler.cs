using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Repositories;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Navigation
{
    public class ItemRequestDeleteCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.ItemRequestDelete;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ILogger<ItemRequestDeleteCallbackHandler> _logger;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly IMenuRepository _menuRepository;
        private readonly ILanguageSettingRepository _languageSettingRepository;
        public ItemRequestDeleteCallbackHandler(
            IUserRepository userRepository,
            IUserService userService,
            ILogger<ItemRequestDeleteCallbackHandler> logger,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            IMenuRepository menuRepository,
            ILanguageSettingRepository languageSettingRepository)
        {
            _userRepository = userRepository;
            _userService = userService;
            _logger = logger;
            _messageService = messageService;
            _localizer = localizer;
            _menuRepository = menuRepository;
            _languageSettingRepository = languageSettingRepository;
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
                _logger.LogWarning("Invalid menuId format: {MenuIdStr}", menuItemIdStr);
                var errorMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.InvalidMenuItemId, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(errorMessage), ct);
                return;
            }

            var menuItem = menu.MenuItems.FirstOrDefault(i => i.Id == menuItemId);
            if (menuItem == null)
            {
                var error = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.ItemNotFound, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(error), ct);
                return;
            }

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

            var template = await ItemDeleteTemplate.CreateAsync(user.LanguageCode, languageCode, _localizer, menuId, menuItemId);
            await _messageService.EditTemplateAsync(chatId, messageId, template, ct);
        }
    }
}
