using System.Collections.Generic;
using System.Threading.Tasks;
using Rat.Contracts;

namespace Rat.Services
{
    public partial interface IEntityValidationService
    {
        /// <summary>
        /// Validate common entity by defined rules
        /// </summary>
        /// <param name="entityName">entity name</param>
        /// <param name="data">inserted entity values</param>
        /// <param name="languageId">language ID (tranlastion of validation messages)</param>
        /// <returns>validation result entries (if empty everything is OK)</returns>
        Task<IList<ValidationEntryResult>> ValidateCommonEntityAsync(
            string entityName, Dictionary<string, object> data, int languageId);
    }
}
