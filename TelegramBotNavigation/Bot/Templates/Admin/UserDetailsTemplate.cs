using TelegramBotNavigation.DTOs;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Services.Interfaces;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;
using static TelegramBotNavigation.Bot.Shared.CallbackKeys;
using System.Text;
using TelegramBotNavigation.Utils;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNavigation.Bot.Shared;

namespace TelegramBotNavigation.Bot.Templates.Admin
{
    public static class UserDetailsTemplate
    {
        public static async Task<TelegramTemplate> CreateAsync(
            LanguageCode userLang,
            ILocalizationManager localizer,
            UserDetailsDto userDto,
            List<UserInteraction> userInteractions,
            int page = 1,
            int pageSize = 10,
            string sort = "desc")
        {
            var fullName = userDto.FirstName;
            if (!string.IsNullOrEmpty(userDto.LastName))
                fullName += $" {userDto.LastName}";

            var sb = new StringBuilder();

            sb.AppendLine($"<b>{fullName}</b>\n");

            sb.AppendLine($"<b>{await localizer.GetInterfaceTranslation(Labels.Language, userLang)}:</b> {userDto.LanguageCode.GetDisplayLabel()}");
            sb.AppendLine($"<b>{await localizer.GetInterfaceTranslation(Labels.LastActivity, userLang)}:</b> {userDto.LastActionTime:g}");

            var statusLabel = await localizer.GetInterfaceTranslation(Labels.Status, userLang);
            var blockedText = userDto.IsBlocked
                ? $"🚫 {await localizer.GetInterfaceTranslation(Labels.Blocked, userLang)}"
                : $"✅ {await localizer.GetInterfaceTranslation(Labels.Unblocked, userLang)}";

            sb.AppendLine($"<b>{statusLabel}:</b> {blockedText}");
            sb.AppendLine($"<b>UserId:</b> <code>{userDto.TelegramUserId}</code>");

            var text = $"{sb.ToString().Trim()}";

            var interactionText = new StringBuilder();

            if (userInteractions.Count > 0)
                interactionText.AppendLine($"<b>{await localizer.GetInterfaceTranslation(Headers.InteractionHistory, userLang)}</b>\n");

            var sortedInteractions = sort == "asc"
                ? userInteractions.OrderBy(i => i.TimeStamp)
                : userInteractions.OrderByDescending(i => i.TimeStamp);

            var paged = sortedInteractions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            foreach (var interaction in paged)
            {
                interactionText.AppendLine(await UserInteractionFormatter.FormatAsync(interaction, userLang, localizer));
                interactionText.AppendLine();
            }

            var finalText = $"{text}\n\n{interactionText.ToString().Trim()}";

            var buttons = new List<InlineKeyboardButton[]>();

            if (!string.IsNullOrEmpty(userDto.Username))
            {
                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithUrl(await localizer.GetInterfaceTranslation(Labels.ViewProfile, userLang), $"https://t.me/{userDto.Username}")
                });
            }

            if (sort == "asc")
            {
                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(await localizer.GetInterfaceTranslation(Labels.SortDesc, userLang), $"{UserDetails}:{userDto.TelegramUserId}:1:desc")
                });
            }
            else
            {
                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(await localizer.GetInterfaceTranslation(Labels.SortAsc, userLang), $"{UserDetails}:{userDto.TelegramUserId}:1:asc")
                });
            }

            int totalPages = (int)Math.Ceiling(userInteractions.Count / (double)pageSize);
            var navButtons = new List<InlineKeyboardButton>();

            if (page > 1)
                navButtons.Add(InlineKeyboardButton.WithCallbackData("«", $"{UserDetails}:{userDto.TelegramUserId}:{page - 1}:{sort}"));

            if (page < totalPages)
                navButtons.Add(InlineKeyboardButton.WithCallbackData("»", $"{UserDetails}:{userDto.TelegramUserId}:{page + 1}:{sort}"));

            if (navButtons.Any())
                buttons.Add(navButtons.ToArray());

            return TelegramTemplate.Create(finalText, inline: new InlineKeyboardMarkup(buttons), removeReplyKeyboard: true);
        }
    }
}
