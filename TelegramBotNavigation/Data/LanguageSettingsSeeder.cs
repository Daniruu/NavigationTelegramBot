using Microsoft.EntityFrameworkCore;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Enums;

namespace TelegramBotNavigation.Data
{
    public static class LanguageSettingsSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.LanguageSettings.AnyAsync()) return;

            var settings = new List<LanguageSetting>
            {
                new() { LanguageCode = LanguageCode.Tr, Priority = 0 },
                new() { LanguageCode = LanguageCode.En, Priority = 1 },
                new() { LanguageCode = LanguageCode.Ru, Priority = 2 },
                new() { LanguageCode = LanguageCode.Pl, Priority = 3 },
            };

            await context.LanguageSettings.AddRangeAsync(settings);
            await context.SaveChangesAsync();
        }
    }
}
