using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;
using static TelegramBotNavigation.Bot.Shared.CallbackKeys;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Navigation
{
    public class NavigationDeleteCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.NavigationDelete;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly IMenuRepository _menuRepository;
        private readonly ILogger<NavigationDeleteCallbackHandler> _logger;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly ITranslationRepository _translationRepository;
        private readonly ICallbackAlertService _callbackAlertService;

        public NavigationDeleteCallbackHandler(
            IUserRepository userRepository,
            IUserService userService,
            IMenuRepository menuRepository,
            ILogger<NavigationDeleteCallbackHandler> logger,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            ITranslationRepository translationRepository,
            ICallbackAlertService callbackAlertService)
        {
            _userRepository = userRepository;
            _userService = userService;
            _menuRepository = menuRepository;
            _logger = logger;
            _messageService = messageService;
            _localizer = localizer;
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
                _logger.LogWarning("User {UserId} not found when trying to access admin command.", userId);
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.NotAdmin, user.LanguageCode);
                var errorTemplate = TelegramTemplate.Create(errorMessage);
                await _messageService.SendTemplateAsync(chatId, errorTemplate, ct);
                return;
            }

            if (args.Length == 0) return;

            var menuIdStr = args[0];
            if (!int.TryParse(menuIdStr, out var menuId))
            {
                _logger.LogWarning("Invalid menuId format: {MenuIdStr}", menuIdStr);
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.InvalidMenuId, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(errorMessage), ct);
                return;
            }

            var menu = await _menuRepository.GetByIdAsync(menuId);
            if (menu == null)
            {
                var error = await _localizer.GetInterfaceTranslation(Errors.MenuNotFound, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(error), ct);
                return;
            }

            await DeleteTranslationsRecursiveAsync(menu);
            await _menuRepository.DeleteAsync(menu);

            // Тут нужно проверить было ли меню основным и переопределить его на первое попавшееся меню
            // Затем обновить все навигационные сообщения

            var alert = await _localizer.GetInterfaceTranslation(Notifications.NavigationDeleteSuccess, user.LanguageCode);
            await _callbackAlertService.ShowAsync(query.Id, alert, cancellationToken: ct);

            var menus = await _menuRepository.GetTopLevelMenusAsync();
            var template = await NavigationManageTemplate.CreateAsync(user.LanguageCode, _localizer, menus);
            await _messageService.EditTemplateAsync(chatId, messageId, template, ct);
        }

        private async Task DeleteTranslationsRecursiveAsync(Menu menu)
        {
            var keys = new List<string>();

            if (!string.IsNullOrEmpty(menu.HeaderTranslationKey))
                keys.Add(menu.HeaderTranslationKey);

            if (!string.IsNullOrEmpty(menu.HeaderImageTranslationKey))
                keys.Add(menu.HeaderImageTranslationKey);

            keys.AddRange(menu.MenuItems.Select(x => x.LabelTranslationKey));
            keys.AddRange(menu.MenuItems
                .Where(x => !string.IsNullOrWhiteSpace(x.MessageTranslationKey))
                .Select(x => x.MessageTranslationKey!));

            foreach (var key in keys.Distinct())
                await _translationRepository.DeleteAllByKeyAsync(key);

            var children = await _menuRepository.GetByParentIdAsync(menu.Id);
            foreach (var child in children)
            {
                await DeleteTranslationsRecursiveAsync(child);
            }
        }

    }
}
