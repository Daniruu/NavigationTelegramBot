using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates.Start;
using TelegramBotNavigation.Repositories;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Common
{
    public class NavigationMessageCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.NavigationMessage;

        private readonly ITelegramMessageService _messageService;
        private readonly IUserRepository _userRepository;
        private readonly IMenuRepository _menuRepository;
        private readonly ILocalizationManager _localizer;
        private readonly IMenuButtonBuilder _buttonBuilder;
        private readonly IUserInteractionService _userInteractionService;
        private readonly IMenuItemRepository _menuItemRepository;

        public NavigationMessageCallbackHandler(
            ITelegramMessageService messageService, 
            IMenuRepository menuRepository,
            IUserRepository userRepository,
            ILocalizationManager localizer,
            IMenuButtonBuilder buttonBuilder,
            IUserInteractionService userInteractionService,
            IMenuItemRepository menuItemRepository)
        {
            _messageService = messageService;
            _menuRepository = menuRepository;
            _userRepository = userRepository;
            _localizer = localizer;
            _buttonBuilder = buttonBuilder;
            _userInteractionService = userInteractionService;
            _menuItemRepository = menuItemRepository;
        }

        public async Task HandleAsync(CallbackQuery query, string[] args, CancellationToken ct)
        {
            if (args.Length == 0 || !int.TryParse(args[0], out var menuId))
                return;

            int? itemId = args.Length >= 2 && int.TryParse(args[1], out var parsedId) ? parsedId : null;

            var menu = await _menuRepository.GetByIdAsync(menuId);
            if (menu == null) return;

            var chatId = query.Message!.Chat.Id;
            var messageId = query.Message!.MessageId;
            var userId = query.From!.Id;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            user.LastActiveAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            if (itemId.HasValue)
            {
                var item = await _menuItemRepository.GetByIdAsync(itemId.Value);
                if (item != null) await _userInteractionService.LogAsync(user, chatId, Enums.ActionType.SubmenuClick, item.LabelTranslationKey);
            }

            var template = await NavigationTemplate.CreateAsync(user.LanguageCode, _localizer, _buttonBuilder, menu);
            await _messageService.EditTemplateAsync(chatId, messageId, template, ct);
        }
    }
}
