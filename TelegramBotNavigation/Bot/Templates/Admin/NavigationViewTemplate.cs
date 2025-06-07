using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Utils;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;
using static TelegramBotNavigation.Bot.Shared.CallbackKeys;


namespace TelegramBotNavigation.Bot.Templates.Admin
{
    public class NavigationViewTemplate
    {
        public static async Task<TelegramTemplate> CreateAsync(
            LanguageCode userLang,
            LanguageCode displayLang,
            ILocalizationManager localizer,
            ILanguageSettingRepository languageSettingRepository,
            IMenuButtonBuilder buttonBuilder,
            Menu menu)
        {
            var headerImage = !string.IsNullOrWhiteSpace(menu.HeaderImageTranslationKey)
                ? await localizer.GetImageTranslationAsync(menu.HeaderImageTranslationKey, displayLang)
                : null;

            var header = !string.IsNullOrWhiteSpace(menu.HeaderTranslationKey)
                ? await localizer.GetCustomTranslationAsync(menu.HeaderTranslationKey, displayLang)
                : null;

            string text = string.Empty;

            if (!string.IsNullOrWhiteSpace(header))
            {
                text = header;
            }
            else if (headerImage == null)
            {
                text = await localizer.GetInterfaceTranslation(Headers.NavigationHeader, displayLang);
            }

            var buttonList = new List<InlineKeyboardButton[]>();

            var navButtons = new List<InlineKeyboardButton[]>();

            foreach (var group in menu.MenuItems.GroupBy(item => item.Row).OrderBy(g => g.Key))
            {
                var row = new List<InlineKeyboardButton>();
                foreach (var item in group.OrderBy(i => i.Order))
                {
                    var button = await buttonBuilder.BuildButtonAsync(item, displayLang, MenuButtonContext.AdminView);
                    row.Add(button);
                }
                navButtons.Add(row.ToArray());
            }

            if (navButtons.Count == 0)
            {
                text = $"{text}\n\n{await localizer.GetInterfaceTranslation(Messages.NavigationHasNoItems, userLang)}";
            }

            var languages = await languageSettingRepository.GetFallbackOrderAsync();
            var languageButtons = languages.Select(lang =>
            {
                var flag = lang switch
                {
                    LanguageCode.Tr => "🇹🇷",
                    LanguageCode.En => "🇬🇧",
                    LanguageCode.Ru => "🇷🇺",
                    LanguageCode.Pl => "🇵🇱",
                    _ => lang.ToString()
                };

                var isSelected = lang == displayLang ? "»" : "";

                return InlineKeyboardButton.WithCallbackData($"{isSelected} {flag}", $"{NavigationView}:{menu.Id}:{lang.ToLanguageTag()}");
            })
            .ToArray();

            var manageButtons = new List<InlineKeyboardButton[]>
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(Labels.EditNavigation, userLang), $"{NavigationEdit}:{menu.Id}:{displayLang.ToLanguageTag()}")
                }
            };

            if (!menu.ParentMenuId.HasValue)
            {
                if (!menu.IsMainMenu)
                {
                    manageButtons.Add(new[]
                    {
                        InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(Labels.NavigationSetDefault, userLang), $"{NavigationSetDefault}:{menu.Id}:{displayLang.ToLanguageTag()}")
                    });
                }
                manageButtons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(Labels.DeleteNavigationMenu, userLang), $"{NavigationRequestDelete}:{menu.Id}"),
                });
            }

            manageButtons.Add(
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    await localizer.GetInterfaceTranslation(Labels.Back, userLang), NavigationManage)
            });

            buttonList.AddRange(navButtons);

            if (menu.ParentMenuId.HasValue)
            {
                buttonList.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(Labels.Back, displayLang),
                        $"{NavigationView}:{menu.ParentMenuId}:{displayLang.ToLanguageTag()}")
                });
            }

            buttonList.Add(languageButtons);
            buttonList.AddRange(manageButtons);

            var markup = new InlineKeyboardMarkup(buttonList);

            return TelegramTemplate.Create(text, inline: markup, removeReplyKeyboard: true, photo: headerImage);
        }
    }
}
