using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rat.Domain;
using Rat.Domain.Entities;

namespace Rat.Services
{
    /// <summary>
    /// Methods working with language entity and other features
    /// </summary>
    public partial class LanguageService : ILanguageService
    {
        private readonly IRepository _repository;

        public LanguageService(
            IRepository repository)
        {
            _repository = repository;
        }

        public virtual async Task<Language> GetDefaultLanguageAsync()
            => (await _repository.GetAllAsync<Language>()).OrderBy(y => y.ItemOrder).FirstOrDefault(x => x.IsActive);

        public virtual async Task<IList<Language>> GetAllAsync()
            => await _repository.GetAllAsync<Language>();
    }
}
