using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;

namespace TelegramBotNavigation.Services.Interfaces
{
    public interface IMenuButtonBuilder
    {
        Task<InlineKeyboardButton> BuildButtonAsync(MenuItem item, LanguageCode lang, MenuButtonContext context);
    }
}
