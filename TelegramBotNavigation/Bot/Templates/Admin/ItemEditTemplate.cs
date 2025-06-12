using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Bot.Templates.Admin
{
    public static class ItemEditTemplate
    {
        public static async Task<TelegramTemplate> CreateAsync(
            LanguageCode userLang,
            LanguageCode displayLang,
            ILocalizationManager localizer,
            ILanguageSettingRepository languageSettingRepository,
            Menu menu,
            MenuItem itemToEdit)
        {
            var langLabel = displayLang.GetDisplayLabel();

            string text = string.Empty;

            var actionButtons = new List<InlineKeyboardButton[]>()
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"✏️ {await localizer.GetInterfaceTranslation(LocalizationKeys.Labels.EditNavigationItemLabel, userLang)}",
                        $"{CallbackKeys.ItemEditLabel}:{menu.Id}:{itemToEdit.Id}:{displayLang.ToLanguageTag()}")
                }
            };

            if (itemToEdit.ActionType == MenuActionType.Url)
            {
                text = await localizer.GetInterfaceTranslation(
                    LocalizationKeys.Headers.NavigationUrlItemEdit, userLang,
                        await localizer.GetCustomTranslationAsync(itemToEdit.LabelTranslationKey, displayLang) ?? "",
                        await localizer.GetCustomTranslationAsync(itemToEdit.LabelTranslationKey, displayLang) ?? "[No Label]",
                        itemToEdit.Url);

            actionButtons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"🔗 {await localizer.GetInterfaceTranslation(LocalizationKeys.Labels.EditNavigationItemUrl, userLang)}",
                        $"{CallbackKeys.ItemEditUrl}:{menu.Id}:{itemToEdit.Id}:{displayLang.ToLanguageTag()}")
                });
            }

            else if (itemToEdit.ActionType == MenuActionType.ShowMessage)
            {
                text = await localizer.GetInterfaceTranslation(
                    LocalizationKeys.Headers.NavigationMessageItemEdit, userLang,
                        await localizer.GetCustomTranslationAsync(itemToEdit.LabelTranslationKey, displayLang) ?? "",
                        await localizer.GetCustomTranslationAsync(itemToEdit.LabelTranslationKey, displayLang) ?? "[No Label]",
                        await localizer.GetCustomTranslationAsync(itemToEdit.MessageTranslationKey!, displayLang) ?? "");

                actionButtons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"📜 {await localizer.GetInterfaceTranslation(LocalizationKeys.Labels.EditNavigationItemMessage, userLang)}",
                        $"{CallbackKeys.ItemEditMessage}:{menu.Id}:{itemToEdit.Id}:{displayLang.ToLanguageTag()}")
                });
            }

            else if (itemToEdit.ActionType == MenuActionType.SubMenu)
            {
                text = await localizer.GetInterfaceTranslation(
                    LocalizationKeys.Headers.NavigationSubmenuItemEdit, userLang,
                    await localizer.GetCustomTranslationAsync(itemToEdit.LabelTranslationKey, displayLang) ?? "",
                    await localizer.GetCustomTranslationAsync(itemToEdit.LabelTranslationKey, displayLang) ?? "[No Label]");
            }

            else if (itemToEdit.ActionType == MenuActionType.SupportRequest)
            {
                text = await localizer.GetInterfaceTranslation(
                    LocalizationKeys.Headers.NaviagationSupportRequestEdit, userLang,
                    await localizer.GetCustomTranslationAsync(itemToEdit.LabelTranslationKey, displayLang) ?? "",
                    await localizer.GetCustomTranslationAsync(itemToEdit.LabelTranslationKey, displayLang) ?? "[No Label]");
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

                return InlineKeyboardButton.WithCallbackData($"{isSelected} {flag}", $"{CallbackKeys.ItemEdit}:{menu.Id}:{itemToEdit.Id}:{lang.ToLanguageTag()}");
            })
            .ToArray();

            var manageButtons = new List<InlineKeyboardButton[]>
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(LocalizationKeys.Labels.Back, userLang), $"{CallbackKeys.NavigationEdit}:{menu.Id}:{displayLang.ToLanguageTag()}")
                }
            };

            var buttonList = new List<InlineKeyboardButton[]>();

            buttonList.AddRange(actionButtons);
            buttonList.Add(languageButtons);
            buttonList.AddRange(manageButtons);

            var markup = new InlineKeyboardMarkup(buttonList);

            return TelegramTemplate.Create(text, inline: markup, removeReplyKeyboard: true);
        }
    }
}
