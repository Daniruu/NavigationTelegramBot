﻿using Telegram.Bot.Types;

namespace TelegramBotNavigation.Bot.CommandHandlers
{
    public interface ICommandHandler
    {
        string Command { get; }
        Task HandleAsync(Message message, string[] args, CancellationToken ct);
    }
}
