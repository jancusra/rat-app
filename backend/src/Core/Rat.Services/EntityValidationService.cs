using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rat.Contracts;
using Rat.Domain;
using Rat.Domain.EntityAttributes;
using Rat.Domain.Extensions;

namespace Rat.Services
{
    /// <summary>
    /// Common entity validation methods
    /// </summary>
    public partial class EntityValidationService : IEntityValidationService
    {
        private readonly IEntityService _entityService;

        private readonly ILocalizationService _localizationService;

        public EntityValidationService(
            IEntityService entityService,
            ILocalizationService localizationService)
        {
            _entityService = entityService;
            _localizationService = localizationService;
        }

        public virtual async Task<IList<ValidationEntryResult>> ValidateCommonEntityAsync(
            string entityName, Dictionary<string, object> data, int languageId)
        {
            var validationEntries = new List<ValidationEntryResult>();
            var entityType = _entityService.GetTableEntityTypeByName(entityName);

            if (!entityType.HasSpecificAttribute<CommonAccessAttribute>())
            {
                return validationEntries;
            }

            var entityProperties = entityType.GetProperties();

            foreach (var entityEntry in data)
            {
                if (entityEntry.Key != nameof(TableEntity.Id))
                {
                    var propertyInfo = entityProperties.FirstOrDefault(x => x.Name == entityEntry.Key);

                    if (propertyInfo != null && propertyInfo.PropertyType == typeof(String)
                        && propertyInfo.HasSpecificAttribute<NotNullableStringAttribute>()
                        && string.IsNullOrEmpty((string)entityEntry.Value))
                    {
                        validationEntries.Add(new ValidationEntryResult {
                            FieldName = entityEntry.Key,
                            Message = string.Format(
                                await _localizationService.GetLocaleAsync(languageId, "RequiredField"),
                                await _localizationService.GetLocaleAsync(languageId, entityEntry.Key))
                        });
                    }
                }
            }

            return validationEntries;
        }
    }
}
