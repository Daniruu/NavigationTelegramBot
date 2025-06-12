using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Utils;
using TelegramBotNavigation.Services.Interfaces;

namespace TelegramBotNavigation.Services
{
    public class TranslationService : ITranslationService
    {
        private readonly ITranslationRepository _translationRepository;

        public TranslationService(ITranslationRepository translationRepository)
        {
            _translationRepository = translationRepository;
        }

        public async Task<string?> GetTranslationAsync(string key, LanguageCode languageCode)
        {
            return await _translationRepository.GetValueAsync(key, languageCode);
        }

        public async Task SetTranslationAsync(string key, LanguageCode languageCode, string value)
        {
            var translation = await _translationRepository.GetAsync(key, languageCode);

            if (translation != null)
            {
                translation.Value = value;
                await _translationRepository.UpdateAsync(translation);
            }
            else
            {
                var newTranslation = new Translation
                {
                    Key = key,
                    Language = languageCode.ToLanguageTag(),
                    Value = value
                };
                await _translationRepository.AddAsync(newTranslation);
            }
        }

        public async Task DeleteTranslationAsync(string key, LanguageCode languageCode)
        {
            await _translationRepository.DeleteAsync(key, languageCode);
        }
    }
}
