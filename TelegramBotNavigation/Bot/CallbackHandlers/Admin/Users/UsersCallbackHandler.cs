namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Users
{
    using Telegram.Bot.Types;
    using TelegramBotNavigation.Bot.Shared;
    using TelegramBotNavigation.Bot.Templates;
    using TelegramBotNavigation.Bot.Templates.Admin;
    using TelegramBotNavigation.Repositories.Interfaces;
    using TelegramBotNavigation.Services.Interfaces;
    using static TelegramBotNavigation.Bot.Shared.CallbackKeys;
    using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;

    public class UsersCallbackHandler : ICallbackHandler
    {
        public string Key => UsersManage;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly ILogger<UsersCallbackHandler> _logger;
        private readonly ICallbackAlertService _callbackAlertService;

        public UsersCallbackHandler(
            IUserRepository userRepository,
            IUserService userService,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            ILogger<UsersCallbackHandler> logger,
            ICallbackAlertService callbackAlertService)
        {
            _userRepository = userRepository;
            _userService = userService;
            _messageService = messageService;
            _localizer = localizer;
            _logger = logger;
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

            int page = 1;
            if (args.Length > 0 && int.TryParse(args[0], out var parsed) && parsed > 0)
                page = parsed;


            var result = await _userRepository.GetPaginatedRecentUsersAsync(page, 10);

            var template = await UsersPageTemplate.CreateAsync(user.LanguageCode, _localizer, result);
            await _messageService.EditTemplateAsync(chatId, messageId, template, ct);
        }
    }
}
