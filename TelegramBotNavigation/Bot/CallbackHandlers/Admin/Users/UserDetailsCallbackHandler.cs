namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Users
{
    using Telegram.Bot.Types;
    using TelegramBotNavigation.Bot.Templates.Admin;
    using TelegramBotNavigation.DTOs;
    using TelegramBotNavigation.Repositories.Interfaces;
    using TelegramBotNavigation.Services.Interfaces;
    using static TelegramBotNavigation.Bot.Shared.CallbackKeys;
    using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;

    public class UserDetailsCallbackHandler : ICallbackHandler
    {
        public string Key => UserDetails;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly ILogger<UserDetailsCallbackHandler> _logger;
        private readonly ICallbackAlertService _callbackAlertService;
        private readonly IUserInteractionService _userInteractionService;

        public UserDetailsCallbackHandler(
            IUserRepository userRepository,
            IUserService userService,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            ILogger<UserDetailsCallbackHandler> logger,
            ICallbackAlertService callbackAlertService,
            IUserInteractionService userInteractionService)
        {
            _userRepository = userRepository;
            _userService = userService;
            _messageService = messageService;
            _localizer = localizer;
            _logger = logger;
            _callbackAlertService = callbackAlertService;
            _userInteractionService = userInteractionService;
        }

        public async Task HandleAsync(CallbackQuery query, string[] args, CancellationToken ct)
        {
            if (args.Length == 0 || !int.TryParse(args[0], out var selectedUserId) || selectedUserId <= 0)
            {
                return;
            }

            bool isRedraw = false;

            int page = 1;
            if (args.Length >= 2 && int.TryParse(args[1], out var parsedPage) && parsedPage > 0)
            {
                page = parsedPage;
                isRedraw = true;
            }
                

            string sort = "desc";
            if (args.Length >= 3 && (args[2] == "asc" || args[2] == "desc"))
            {
                sort = args[2];
                isRedraw = true;
            }

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

            var selectedUser = await _userRepository.GetByIdAsync(selectedUserId);
            if (selectedUser == null) return;

            var userInteractions = await _userInteractionService.GetByUserIdAsync(selectedUser.Id);

            var selectedUserDto = new UserDetailsDto
            {
                TelegramUserId = selectedUser.Id,
                FirstName = selectedUser.FirstName,
                LastName = selectedUser.LastName,
                Username = selectedUser.Username,
                LanguageCode = selectedUser.LanguageCode,
                IsBlocked = selectedUser.IsBlocked,
                ChatId = selectedUser.ChatId,
                LastActionTime = selectedUser.LastActiveAt,
                TotalActions = userInteractions.Count(),
            };

            var template = await UserDetailsTemplate.CreateAsync(
                user.LanguageCode,
                _localizer,
                selectedUserDto,
                userInteractions,
                page,
                pageSize: 10,
                sort);

            if (isRedraw)
            {
                await _messageService.EditTemplateAsync(chatId, messageId, template, ct);
            }
            else
            {
                await _messageService.SendTemplateAsync(chatId, template, ct);
            }
        }
    }
}
