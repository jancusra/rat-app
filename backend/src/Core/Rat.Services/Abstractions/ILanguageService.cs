using System.Collections.Generic;
using System.Threading.Tasks;
using Rat.Domain.Entities;

namespace Rat.Services
{
    public partial interface ILanguageService
    {
        /// <summary>
        /// Get default language by configured order and activity
        /// </summary>
        /// <returns>default language entity</returns>
        Task<Language> GetDefaultLanguageAsync();

        /// <summary>
        /// Get all stored languages
        /// </summary>
        /// <returns>list of all language entities</returns>
        Task<IList<Language>> GetAllAsync();
    }
}
