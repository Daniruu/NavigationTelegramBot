using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Services.Interfaces;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;
using static TelegramBotNavigation.Bot.Shared.CallbackKeys;

namespace TelegramBotNavigation.Bot.Templates.Admin
{
    public static class NavigationManageTemplate
    {
        public static async Task<TelegramTemplate> CreateAsync(
            LanguageCode lang,
            ILocalizationManager localizer,
            IEnumerable<Menu> menus)
        {
            var text = string.Empty;

            if (menus.Any())
            {
                text = await localizer.GetInterfaceTranslation(Headers.NavigationManage, lang);
            }
            else
            {
                text = await localizer.GetInterfaceTranslation(Headers.NavigationManageEmpty, lang);
            }


            var buttonList = new List<InlineKeyboardButton[]>();

            foreach (var menu in menus)
            {
                var title = menu.Title;

                if (menu.IsMainMenu)
                {
                    title = $"🌟 {title}";
                }

                buttonList.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(title, $"{NavigationView}:{menu.Id}")
                });
            }

            buttonList.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(Labels.AddNavigationMenu, lang), NavigationAdd)
            });

            buttonList.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(Labels.Back, lang), AdminPanel)
            });

            var markup = new InlineKeyboardMarkup(buttonList);

            return TelegramTemplate.Create(text, inline: markup, removeReplyKeyboard: true);
        }
    }
}
