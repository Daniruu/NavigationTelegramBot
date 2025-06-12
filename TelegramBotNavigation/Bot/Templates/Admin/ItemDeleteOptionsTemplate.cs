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
    public static class ItemDeleteOptionsTemplate
    {
        public static async Task<TelegramTemplate> CreateAsync(
            LanguageCode userLang,
            LanguageCode displayLang,
            ILocalizationManager localizer,
            ILanguageSettingRepository languageSettingRepository,
            Menu menu)
        {
            var headerImage = !string.IsNullOrWhiteSpace(menu.HeaderImageTranslationKey)
                    ? await localizer.GetImageTranslationAsync(menu.HeaderImageTranslationKey, displayLang)
                    : null;

            var header = !string.IsNullOrWhiteSpace(menu.HeaderTranslationKey)
                ? await localizer.GetCustomTranslationAsync(menu.HeaderTranslationKey, displayLang)
                : null;

            var text = await localizer.GetInterfaceTranslation(Headers.NavigationDeleteItem, userLang, menu.Title); 

            var navButtons = new List<InlineKeyboardButton[]>();

            foreach (var group in menu.MenuItems.GroupBy(item => item.Row).OrderBy(g => g.Key))
            {
                var row = new List<InlineKeyboardButton>();
                foreach (var item in group.OrderBy(i => i.Order))
                {
                    var button = InlineKeyboardButton.WithCallbackData(
                        $"❌ {await localizer.GetCustomTranslationAsync(item.LabelTranslationKey, displayLang)}" ?? "❌ [No label]", 
                        $"{CallbackKeys.ItemRequestDelete}:{menu.Id}:{item.Id}:{displayLang.ToLanguageTag()}");
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

                return InlineKeyboardButton.WithCallbackData($"{isSelected} {flag}", $"{ItemDeleteOptions}:{menu.Id}:{lang.ToLanguageTag()}");
            })
            .ToArray();

            var manageButtons = new List<InlineKeyboardButton[]>();

            manageButtons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    await localizer.GetInterfaceTranslation(Labels.AddNavigationItem, userLang), $"{NavigationItemAdd}:{menu.Id}:{displayLang.ToLanguageTag()}")
            });

            manageButtons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    await localizer.GetInterfaceTranslation(Labels.NavigationHeaderEdit, userLang), $"{NavigationHeaderEdit}:{menu.Id}:{displayLang.ToLanguageTag()}")
            });

            if (headerImage != null)
            {
                manageButtons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(Labels.NavigationHeaderEditImage, userLang), $"{NavigationHeaderImageEdit}:{menu.Id}:{displayLang.ToLanguageTag()}"),
                    InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(Labels.NavigationDeleteHeaderImage, userLang), $"{DeleteHeaderImageConfirmation}:{menu.Id}:{displayLang.ToLanguageTag()}"),
                });
            }
            else
            {
                manageButtons.Add(
                [
                    InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(Labels.NavigationHeaderEditImage, userLang), $"{NavigationHeaderImageEdit}:{menu.Id}:{displayLang.ToLanguageTag()}"),
                ]);
            }

            manageButtons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(Labels.ReorderNavigation, userLang), $"{NavigationReorder}:{menu.Id}:{displayLang.ToLanguageTag()}")
            });

            manageButtons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(Labels.EditMode, userLang), $"{NavigationEdit}:{menu.Id}:{displayLang.ToLanguageTag()}")
            });

            manageButtons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(Labels.Back, userLang), $"{NavigationView}:{menu.Id}:{displayLang.ToLanguageTag()}")
            });

            var buttonList = new List<InlineKeyboardButton[]>();

            buttonList.AddRange(navButtons);
            buttonList.Add(languageButtons);
            buttonList.AddRange(manageButtons);

            var markup = new InlineKeyboardMarkup(buttonList);

            return TelegramTemplate.Create(text, inline: markup, removeReplyKeyboard: true, photo: headerImage);
        }
    }
}
