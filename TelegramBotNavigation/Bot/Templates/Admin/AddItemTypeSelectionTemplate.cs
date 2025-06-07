using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Bot.Templates.Admin
{
    public static class AddItemTypeSelectionTemplate
    {
        public static async Task<TelegramTemplate> CreateAsync(
            LanguageCode userLang,
            LanguageCode displayLang,
            ILocalizationManager localizer,
            ILanguageSettingRepository languageSettingRepository,
            Menu menu)
        {
            var text = $"<b>{await localizer.GetInterfaceTranslation(LocalizationKeys.Headers.ItemTypeSelection, userLang)}</b>";

            var actionButtons = new List<InlineKeyboardButton[]>
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(LocalizationKeys.Labels.MenuItemTypeUrl, userLang), $"{CallbackKeys.NavigationItemSelectType}:{MenuActionType.Url}")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(LocalizationKeys.Labels.MenuItemTypeShowMessage, userLang), $"{CallbackKeys.NavigationItemSelectType}:{MenuActionType.ShowMessage}")
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(LocalizationKeys.Labels.MenuItemTypeSubMenu, userLang), $"{CallbackKeys.NavigationItemSelectType}:{MenuActionType.SubMenu}")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(LocalizationKeys.Labels.MenuItemTypeSupportRequest, userLang), $"{CallbackKeys.NavigationItemSelectType}:{MenuActionType.SupportRequest}")
                }
            };

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

                return InlineKeyboardButton.WithCallbackData($"{isSelected} {flag}", $"{CallbackKeys.NavigationItemAdd}:{menu.Id}:{lang.ToLanguageTag()}");
            })
            .ToArray();

            var manageButtons = new List<InlineKeyboardButton[]>
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(LocalizationKeys.Labels.Cancel, userLang), $"{CallbackKeys.NavigationEdit}:{menu.Id}:{displayLang}")
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
