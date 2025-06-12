using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Services.Interfaces;

namespace TelegramBotNavigation.Bot.Templates.Admin
{
    public class UnsetAdminTemplate
    {
        public static async Task<TelegramTemplate> CreateAsync(LanguageCode userLang, ILocalizationManager localizer, TelegramUser user)
        {
            var text = await localizer.GetInterfaceTranslation(Messages.UnsetUserAdmin, userLang);

            var fullName = user.FirstName;
            if (!string.IsNullOrEmpty(user.LastName))
                fullName += $" {user.LastName}";

            var sb = new StringBuilder();
            sb.AppendLine(fullName);
            if (user.Username != null) sb.AppendLine($"<b>Username:</b> <code>{user.Username}</code>");
            sb.AppendLine($"<b>UserId:</b> <code>{user.Id}</code>");

            text += $"\n\n{sb.ToString().Trim()}";

            var markup = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(await localizer.GetInterfaceTranslation(Labels.UnsetAdmin, userLang), $"{CallbackKeys.UnsetAdmin}:{user.Id}")
                }
            });

            return TelegramTemplate.Create(text, markup);
        }
    }
}
