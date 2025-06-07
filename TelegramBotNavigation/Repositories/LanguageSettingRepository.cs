using Microsoft.EntityFrameworkCore;
using TelegramBotNavigation.Data;
using TelegramBotNavigation.Enums;
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
    }
}
