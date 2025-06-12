using Microsoft.EntityFrameworkCore;
using TelegramBotNavigation.Data;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;

namespace TelegramBotNavigation.Repositories
{
    public class BotSettingsRepository : IBotSettingsRepository
    {
        private readonly ApplicationDbContext _context;

        public BotSettingsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string?> GetValueAsync(string key)
        {
            return await _context.BotSettings
                .Where(s => s.Key == key)
                .Select(s => s.Value)
                .FirstOrDefaultAsync();
        }

        public async Task SetValueAsync(string key, string value)
        {
            var setting = await _context.BotSettings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting == null)
            {
                _context.BotSettings.Add(new BotSetting { Key = key, Value = value });
            }
            else
            {
                setting.Value = value;
            }

            await _context.SaveChangesAsync();
        }
    }
}
