using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rat.Services;

namespace Rat.Endpoint.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public partial class LocalizationController : ControllerBase
    {
        private readonly ILocalizationService _localizationService;

        public LocalizationController(
            ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        /// <summary>
        /// Get localizations by specific language ID
        /// </summary>
        /// <param name="languageId">language ID</param>
        /// <returns>dictionary of all filtered localizations</returns>
        [HttpGet]
        public virtual async Task<IActionResult> GetByLanguageId(int languageId)
        {
            // Group by name so duplicate localization names for the same language don't throw;
            // the first value wins.
            var locales = (await _localizationService.GetByLanguageIdAsync(languageId))
                .GroupBy(x => x.Name)
                .ToDictionary(g => g.Key, g => g.First().Value);

            return Ok(locales);
        }
    }
}
