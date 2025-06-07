using System.Threading.Tasks;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Services.Interfaces;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;

namespace TelegramBotNavigation.Utils
{
    public static class UserInteractionFormatter
    {
        public static async Task<string> FormatAsync(UserInteraction interaction, LanguageCode userLang, ILocalizationManager localizer)
        {
            var time = interaction.TimeStamp.ToString("g"); // или "dd.MM.yyyy HH:mm"
            var actionText = interaction.ActionType switch
            {
                ActionType.Command => await localizer.GetInterfaceTranslation(Messages.UserExecuteCommand, userLang, interaction.Value),

                ActionType.SubmenuClick => await localizer.GetInterfaceTranslation(Messages.UserClickSubmenu, userLang, 
                    await localizer.GetCustomTranslationAsync(interaction.Value, userLang) ?? await localizer.GetInterfaceTranslation(Errors.ItemNotFound, userLang)),

                ActionType.ShowMessageClick => await localizer.GetInterfaceTranslation(Messages.UserClickShowMessage, userLang,
                    await localizer.GetCustomTranslationAsync(interaction.Value, userLang) ?? await localizer.GetInterfaceTranslation(Errors.ItemNotFound, userLang)),

                ActionType.LanguageChange => await localizer.GetInterfaceTranslation(Messages.UserChangedLanguage, userLang, interaction.Value),

                ActionType.Message => await localizer.GetInterfaceTranslation(Messages.UserSentMessage, userLang, 
                    await localizer.GetCustomTranslationAsync(interaction.Value, userLang) ?? await localizer.GetInterfaceTranslation(Errors.ItemNotFound, userLang)),

                ActionType.SupportRequest => await localizer.GetInterfaceTranslation(Messages.UserRequestedSupport, userLang),

                _ => await localizer.GetInterfaceTranslation(Messages.UserInteractionUnknown, userLang, interaction.Value)
            };

            return $"<i>{time}</i>\n{actionText}";
        }
    }
}
