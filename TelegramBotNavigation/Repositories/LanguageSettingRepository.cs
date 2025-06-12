using Microsoft.EntityFrameworkCore;
using TelegramBotNavigation.Data;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;

namespace TelegramBotNavigation.Repositories
{
    public class LanguageSettingRepository : ILanguageSettingRepository
    {
        private readonly ApplicationDbContext _context;
        public LanguageSettingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<LanguageCode>> GetFallbackOrderAsync()
        {
            return await _context.LanguageSettings
                .OrderBy(s => s.Priority)
                .Select(s => s.LanguageCode)
                .ToListAsync();
        }

        public async Task<LanguageSetting?> GetByIdAsync(int id)
        {
            return await _context.LanguageSettings.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<LanguageSetting>> GetLanguageSettingsAsync()
        {
            return await _context.LanguageSettings
                .OrderBy(s => s.Priority)
                .ToListAsync();
        }

        public async Task UpdateAsync(LanguageSetting languageSetting)
        {
            _context.LanguageSettings.Update(languageSetting);
            await _context.SaveChangesAsync();
        }
    }
}
