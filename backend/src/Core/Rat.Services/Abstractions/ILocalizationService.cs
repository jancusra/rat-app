using System.Collections.Generic;
using System.Threading.Tasks;
using Rat.Domain.Entities;

namespace Rat.Services
{
    public partial interface ILocalizationService
    {
        /// <summary>
        /// Get translation by language ID and name
        /// </summary>
        /// <param name="languageId">language ID</param>
        /// <param name="localName">localization name</param>
        /// <returns>final translated string</returns>
        Task<string> GetLocaleAsync(int languageId, string localName);

        /// <summary>
        /// Get all translations by language ID
        /// </summary>
        /// <param name="languageId">language ID</param>
        /// <returns>list of all language translations</returns>
        Task<IList<Localization>> GetByLanguageIdAsync(int languageId);
    }
}
