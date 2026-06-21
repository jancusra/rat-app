using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using Rat.Domain;
using Rat.Domain.Entities;

namespace Rat.Services
{
    /// <summary>
    /// Methods working with localization entity and other features
    /// </summary>
    public partial class LocalizationService : ILocalizationService
    {
        private readonly IRepository _repository;

        private readonly ILanguageService _languageService;

        public LocalizationService(
            IRepository repository,
            ILanguageService languageService)
        {
            _repository = repository;
            _languageService = languageService;
        }

        public virtual async Task<string> GetLocaleAsync(int languageId, string localName)
        {
            var localeValue = await _repository.Table<Localization>()
                .FirstOrDefaultAsync(x => x.LanguageId == languageId && x.Name == localName);

            if (localeValue == null)
            {
                return localName;
            }
            else
            {
                return localeValue.Value;
            }
        }

        public virtual async Task<IList<Localization>> GetByLanguageIdAsync(int languageId)
        {
            var langId = await GetLanguageIdAsync(languageId);
            return await _repository.Table<Localization>().Where(x => x.LanguageId == langId).ToListAsync();
        }

        /// <summary>
        /// Get default language ID if ID is not specified
        /// </summary>
        /// <param name="languageId">language ID</param>
        /// <returns>final language ID</returns>
        private async Task<int> GetLanguageIdAsync(int languageId)
        {
            if (languageId > default(int))
            {
                return languageId;
            }
            else
            {
                return (await _languageService.GetDefaultLanguageAsync()).Id;
            }
        }
    }
}
