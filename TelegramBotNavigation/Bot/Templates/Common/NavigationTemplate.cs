using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Services.Interfaces;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;

namespace TelegramBotNavigation.Bot.Templates.Start
{
    public static class NavigationTemplate
    {
        public static async Task<TelegramTemplate> CreateAsync(
            LanguageCode userLang,
            ILocalizationManager localizer,
            IMenuButtonBuilder buttonBuilder,
            Menu menu)
        {
            var headerImage = !string.IsNullOrWhiteSpace(menu.HeaderImageTranslationKey)
                ? await localizer.GetImageTranslationAsync(menu.HeaderImageTranslationKey, userLang)
                : null;

            var header = !string.IsNullOrWhiteSpace(menu.HeaderTranslationKey)
               ? await localizer.GetCustomTranslationAsync(menu.HeaderTranslationKey, userLang)
               : null;

            string text = string.Empty;

            if (!string.IsNullOrWhiteSpace(header))
            {
                text = header;
            }
            else if (headerImage == null)
            {
                text = await localizer.GetInterfaceTranslation(Headers.NavigationHeader, userLang);
            }

            var navButtons = new List<InlineKeyboardButton[]>();

            foreach (var group in menu.MenuItems.GroupBy(item => item.Row).OrderBy(g => g.Key))
            {
                var row = new List<InlineKeyboardButton>();
                foreach (var item in group.OrderBy(i => i.Order))
                {
                    var button = await buttonBuilder.BuildButtonAsync(item, userLang, MenuButtonContext.User);
                    row.Add(button);
                }
                navButtons.Add(row.ToArray());
            }

            if (navButtons.Count == 0)
            {
                text = $"{text}\n\n{await localizer.GetInterfaceTranslation(Messages.NavigationHasNoItems, userLang)}";
            }

            if (menu.ParentMenuId.HasValue)
            {
                string callback = $"{CallbackKeys.NavigationMessage}:{menu.ParentMenuId}";

                navButtons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(Labels.Back, userLang),
                        callback)
                });
            }

            var markup = new InlineKeyboardMarkup(navButtons);

            return TelegramTemplate.Create(text, inline: markup, removeReplyKeyboard: true, photo: headerImage);
        }
    }
}
