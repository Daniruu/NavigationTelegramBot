using Microsoft.EntityFrameworkCore;
using TelegramBotNavigation.Data;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Repositories
{
    public class TranslationRepository : ITranslationRepository
    {
        private readonly ApplicationDbContext _context;

        public TranslationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string?> GetValueAsync(string key, LanguageCode languageCode)
        {
            return await _context.Translations.Where(t => t.Key == key && t.Language == languageCode.ToLanguageTag())
                .Select(t => t.Value)
                .FirstOrDefaultAsync();
        }

        public async Task<Translation?> GetAsync(string key, LanguageCode languageCode)
        {
            return await _context.Translations.FirstOrDefaultAsync(t => t.Key == key && t.Language == languageCode.ToLanguageTag());
        }

        public async Task AddAsync(Translation translation)
        {
            await _context.Translations.AddAsync(translation);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Translation translation)
        {
            _context.Translations.Update(translation);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string key, LanguageCode languageCode)
        {
            var translation = await _context.Translations
                .FirstOrDefaultAsync(t => t.Key == key && t.Language == languageCode.ToLanguageTag());

            if (translation != null)
            {
                _context.Translations.Remove(translation);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAllByKeyAsync(string key)
        {
            var translations = await _context.Translations
                .Where(t => t.Key == key)
                .ToListAsync();
            if (translations.Any())
            {
                _context.Translations.RemoveRange(translations);
                await _context.SaveChangesAsync();
            }
        }
    }
}
