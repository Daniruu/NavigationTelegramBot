using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using System;

namespace TelegramBotNavigation.Bot.CommandHandlers.Admin
{
    public class SetGroupCommandHandler : ICommandHandler
    {
        public string Command => "/setgroup";

        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly ILogger<SetGroupCommandHandler> _logger;
        private readonly IBotSettingsService _settingsService;

        public SetGroupCommandHandler(
            IUserService userService,
            IUserRepository userRepository,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            ILogger<SetGroupCommandHandler> logger,
            IBotSettingsService settingsService)
        {
            _userService = userService;
            _userRepository = userRepository;
            _messageService = messageService;
            _localizer = localizer;
            _logger = logger;
            _settingsService = settingsService;
        }
        public async Task HandleAsync(Message message, string[] args, CancellationToken ct)
        {
            var chat = message.Chat;
            if (!chat.IsForum) return;

            var chatId = message.Chat.Id;
            var userId = message.From!.Id;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            if (!_userService.IsAdmin(user))
            {
                _logger.LogWarning("Access denied for user {UserId} when attempting to execute an admin command: {command}.", userId, Command);
                var errorMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.NotAdmin, user.LanguageCode);
                var errorTemplate = TelegramTemplate.Create(errorMessage);
                await _messageService.SendTemplateAsync(chatId, errorTemplate, ct);
                return;
            }

            await _settingsService.SetSupportGroupIdAsync(chatId);

            
            await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create("✅ Этот чат сохранён как группа поддержки."), ct);
        }
    }
}
