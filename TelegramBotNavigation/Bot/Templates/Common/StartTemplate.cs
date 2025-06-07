using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Services.Interfaces;

namespace TelegramBotNavigation.Bot.Templates.Start
{
    public static class StartTemplate
    {
        public static async Task<TelegramTemplate> CreateAsync(LanguageCode lang, IWelcomeMessageProvider welcomeMessageProvider)
        {
            var welcomeMessage = await welcomeMessageProvider.GetForUserAsync(lang);

            var text = welcomeMessage.Text;
            var welcomePhoto = welcomeMessage.ImageFileId;

            return TelegramTemplate.Create(text, photo: welcomePhoto);
        }
    }
}
