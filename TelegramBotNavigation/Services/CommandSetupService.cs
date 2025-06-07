using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Services.Interfaces;

namespace TelegramBotNavigation.Services
{
    public class CommandSetupService : ICommandSetupService
    {
        private readonly ITelegramBotClient _bot;
        private readonly IUserService _userService;
        private readonly ILocalizationManager _localizer;

        public CommandSetupService(ITelegramBotClient bot, IUserService userService, ILocalizationManager localizer)
        {
            _bot = bot;
            _userService = userService;
            _localizer = localizer;
        }
        public async Task SetupCommandsAsync(long chatId, TelegramUser user, CancellationToken ct)
        {
            var isAdmin = await _userService.IsAdminAsync(user.Id);

            var commands = new List<BotCommand>
            {
                new BotCommand
                {
                    Command = "start",
                    Description = await _localizer.GetInterfaceTranslation(LocalizationKeys.Commands.Start, user.LanguageCode)
                },
                new BotCommand
                {
                    Command = "language",
                    Description = await _localizer.GetInterfaceTranslation(LocalizationKeys.Commands.Language, user.LanguageCode)
                },
                new BotCommand
                {
                    Command = "navigation",
                    Description = await _localizer.GetInterfaceTranslation(LocalizationKeys.Commands.Navigation, user.LanguageCode)
                }
            };

            if (isAdmin)
            {
                commands.Add(new BotCommand
                {
                    Command = "admin",
                    Description = await _localizer.GetInterfaceTranslation(LocalizationKeys.Commands.Admin, user.LanguageCode)
                });
            }

            await _bot.SetMyCommands(commands, scope: BotCommandScope.Chat(chatId), cancellationToken: ct);
        }
    }
}
