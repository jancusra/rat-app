using System.Collections.Generic;
using Rat.Contracts.Common;
using Rat.Domain.Types;

namespace Rat.Mappings.CommonEntityEntries
{
    public static class User
    {
        /// <summary>
        /// Configured metadata to get/edit user entity
        /// </summary>
        /// <returns>list of configured data</returns>
        public static IList<EntityEntryDto> GetMetadata()
        {
            return new List<EntityEntryDto>()
            {
                new EntityEntryDto()
                {
                    Name = nameof(Domain.Entities.UserRole),
                    EntryType = CustomEntityEntryType.MappedMultiSelect.ToString(),
                    Order = 2
                },
                new EntityEntryDto()
                {
                    Name = nameof(Domain.Entities.User.UserGuid),
                    Excluded = true
                },
                new EntityEntryDto()
                {
                    Name = nameof(Domain.Entities.User.Deleted),
                    Excluded = true
                },
                new EntityEntryDto()
                {
                    Name = nameof(Domain.Entities.User.InvalidLoginAttempts),
                    Excluded = true
                },
                new EntityEntryDto()
                {
                    Name = nameof(Domain.Entities.User.LastIpAddress),
                    Excluded = true
                },
                new EntityEntryDto()
                {
                    Name = nameof(Domain.Entities.User.LastLoginUTC),
                    Excluded = true
                }
            };
        }
    }
}
