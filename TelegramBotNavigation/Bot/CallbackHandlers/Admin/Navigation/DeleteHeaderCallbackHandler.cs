using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;
using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Navigation
{
    public class DeleteHeaderCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.DeleteHeader;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly ILogger<DeleteHeaderCallbackHandler> _logger;
        private readonly IMenuRepository _menuRepository;
        private readonly ILanguageSettingRepository _languageSettingRepository;
        private readonly ITranslationService _translationService;
        private readonly ICallbackAlertService _callbackAlertService;

        public DeleteHeaderCallbackHandler(
            IUserRepository userRepository,
            IUserService userService,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            ILogger<DeleteHeaderCallbackHandler> logger,
            IMenuRepository menuRepository,
            ILanguageSettingRepository languageSettingRepository,
            ITranslationService translationService,
            ICallbackAlertService callbackAlertService)
        {
            _userRepository = userRepository;
            _userService = userService;
            _messageService = messageService;
            _localizer = localizer;
            _logger = logger;
            _menuRepository = menuRepository;
            _languageSettingRepository = languageSettingRepository;
            _translationService = translationService;
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
                var error = await _localizer.GetInterfaceTranslation(Errors.MenuNotFound, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(error), ct);
                return;
            }

            var language = args[1];
            LanguageCode languageCode = LanguageCodeHelper.FromTelegramTag(language);

            await _translationService.DeleteTranslationAsync(menu.HeaderTranslationKey, languageCode);

            var successMessage = await _localizer.GetInterfaceTranslation(Notifications.HeaderImageDeleted, user.LanguageCode, menu.Title);
            await _callbackAlertService.ShowAsync(query.Id, successMessage, cancellationToken: ct);

            var template = await NavigationEditTemplate.CreateAsync(user.LanguageCode, languageCode, _localizer, _languageSettingRepository, menu);
            await _messageService.EditTemplateAsync(chatId, messageId, template, ct);
        }
    }
}
