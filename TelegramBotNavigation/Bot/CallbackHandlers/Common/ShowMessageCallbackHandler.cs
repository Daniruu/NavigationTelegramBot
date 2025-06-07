using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Common
{
    public class ShowMessageCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.ShowMessage;

        private readonly IUserRepository _userRepository;
        private readonly ILocalizationManager _localizer;
        private readonly ITelegramMessageService _messageService;
        private readonly ILogger<ShowMessageCallbackHandler> _logger;
        private readonly IUserInteractionService _userInteractionService;
        private readonly IMenuItemRepository _menuItemRepository;


        public ShowMessageCallbackHandler(
            IUserRepository userRepository,
            ILocalizationManager localizer,
            ITelegramMessageService messageService,
            ILogger<ShowMessageCallbackHandler> logger,
            ICommandSetupService commandSetupService,
            INavigationMessageService navigationMessageService,
            IUserInteractionService userInteractionService,
            IMenuItemRepository menuItemRepository)
        {
            _userRepository = userRepository;
            _localizer = localizer;
            _messageService = messageService;
            _logger = logger;
            _userInteractionService = userInteractionService;
            _menuItemRepository = menuItemRepository;
        }

        public async Task HandleAsync(CallbackQuery query, string[] args, CancellationToken ct)
        {
            if (args.Length == 0 || !int.TryParse(args[0], out var itemId) || itemId <= 0) return;

            var item = await _menuItemRepository.GetByIdAsync(itemId);
            if (item == null) return;

            var messageTranslationKey = item.MessageTranslationKey;
            if (messageTranslationKey == null) return;

            var labelTranslationKey = item.LabelTranslationKey;
            if (labelTranslationKey == null) return;

            var chatId = query.Message!.Chat.Id;
            var messageId = query.Message.MessageId;
            var userId = query.From.Id;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            user.LastActiveAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var languageCode = user.LanguageCode;

            if (args.Length == 2)
            {
                languageCode = LanguageCodeHelper.FromTelegramTag(args[1]);
            }

            var message = await _localizer.GetCustomTranslationAsync(messageTranslationKey, languageCode);
            if (message == null) return;

            var template = new TelegramTemplate
            {
                Text = message,
                ReplyMarkup = null
            };

            await _userInteractionService.LogAsync(query.From, chatId, Enums.ActionType.ShowMessageClick, labelTranslationKey);

            await _messageService.SendTemplateAsync(chatId, template, ct);
        }
    }
}
