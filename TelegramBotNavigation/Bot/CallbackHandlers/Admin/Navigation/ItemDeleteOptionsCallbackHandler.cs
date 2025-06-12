using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Utils;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Navigation
{
    public class ItemDeleteOptionsCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.ItemDeleteOptions;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ILogger<NavigationViewCallbackHandler> _logger;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly IMenuRepository _menuRepository;
        private readonly ILanguageSettingRepository _languageSettingRepository;
        private readonly ICallbackAlertService _callbackAlertService;

        public ItemDeleteOptionsCallbackHandler(
            IUserRepository userRepository, 
            IUserService userService, 
            ILogger<NavigationViewCallbackHandler> logger, 
            ITelegramMessageService messageService, 
            ILocalizationManager localizer, 
            IMenuRepository menuRepository, 
            ILanguageSettingRepository languageSettingRepository,
            ICallbackAlertService callbackAlertService)
        {
            _userRepository = userRepository;
            _userService = userService;
            _logger = logger;
            _messageService = messageService;
            _localizer = localizer;
            _menuRepository = menuRepository;
            _languageSettingRepository = languageSettingRepository;
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
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.NotAdmin, user.LanguageCode);
                await _callbackAlertService.ShowAsync(query.Id, errorMessage, cancellationToken: ct);
                return;
            }

            if (args.Length == 0) return;

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

            var template = await ItemDeleteOptionsTemplate.CreateAsync(user.LanguageCode, languageCode, _localizer, _languageSettingRepository, menu);
            await _messageService.EditTemplateAsync(chatId, messageId, template, ct);
        }
    }
}
