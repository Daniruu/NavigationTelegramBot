using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNavigation.DTOs;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Services.Interfaces;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;
using static TelegramBotNavigation.Bot.Shared.CallbackKeys;
using TelegramBotNavigation.Utils;
using TelegramBotNavigation.Models;
using System.Reflection.Emit;

namespace TelegramBotNavigation.Bot.Templates.Admin
{
    public static class UsersPageTemplate
    {
        public static async Task<TelegramTemplate> CreateAsync(
            LanguageCode userLang, 
            ILocalizationManager localizer,
            PaginatedUserListDto dto)
        {
            var text = $"{await localizer.GetInterfaceTranslation(Headers.UsersManage, userLang)} ({dto.CurrentPage}/{dto.TotalPages})";

            var buttonList = new List<InlineKeyboardButton[]>();

            var userButtons = new List<InlineKeyboardButton[]>();

            foreach (var user in dto.Users)
            {
                var fullName = user.FirstName;

                if (!string.IsNullOrEmpty(user.LastName))
                    fullName += $" {user.LastName}";

                if (!string.IsNullOrEmpty(user.Username))
                    fullName += $" (@{user.Username})";

                var userButton = InlineKeyboardButton.WithCallbackData(
                    fullName,
                    $"{UserDetails}:{user.TelegramUserId}");

                userButtons.Add(new[] { userButton });
            }

            buttonList.AddRange(userButtons);

            if (dto.TotalPages > 1)
            {
                var pageButtons = new List<InlineKeyboardButton>();
                var navButtons = new List<InlineKeyboardButton>();

                int current = dto.CurrentPage;
                int total = dto.TotalPages;

                int maxPageButtons = 8;

                int half = maxPageButtons / 2;
                int start = Math.Max(1, current - half);
                int end = start + maxPageButtons - 1;

                if (end > total)
                {
                    end = total;
                    start = Math.Max(1, end - maxPageButtons + 1);
                }

                for (int i = start; i <= end; i++)
                {
                    string label = i == current ? $"·{i}·" : i.ToString();
                    pageButtons.Add(InlineKeyboardButton.WithCallbackData(label, $"{UsersManage}:{i}"));
                }

                if (current > 1)
                    navButtons.Add(InlineKeyboardButton.WithCallbackData("«", $"{UsersManage}:{current - 1}"));

                if (current < total)
                    navButtons.Add(InlineKeyboardButton.WithCallbackData("»", $"{UsersManage}:{current + 1}"));

                buttonList.Add(pageButtons.ToArray());

                if (navButtons.Count > 0)
                    buttonList.Add(navButtons.ToArray());
            }


            var manageButtons = new List<InlineKeyboardButton[]>()
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(Labels.Back, userLang), AdminPanel)
                }
            };

            buttonList.AddRange(manageButtons);

            var markup = new InlineKeyboardMarkup(buttonList);

            return TelegramTemplate.Create(text, inline: markup, removeReplyKeyboard: true);
        }
    }
}
