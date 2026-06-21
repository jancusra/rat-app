using System.Collections.Generic;
using Rat.Contracts.Common;
using Rat.Domain.Types;

namespace Rat.Mappings.CommonTableColumns
{
    public static class User
    {
        /// <summary>
        /// Configured metadata to display users in a table
        /// </summary>
        /// <returns>list of configured data</returns>
        public static IList<ColumnMetadata> GetMetadata()
        {
            return new List<ColumnMetadata>()
            {
                new ColumnMetadata()
                {
                    Name = nameof(Domain.Entities.User.IsVatPayer),
                    Hidden = true
                },
                new ColumnMetadata()
                {
                    Name = CustomColumnType.EditInViewButton.ToString(),
                    EntryType = CustomColumnType.EditInViewButton.ToString()
                },
                new ColumnMetadata()
                {
                    Name = CustomColumnType.DeleteButton.ToString(),
                    EntryType = CustomColumnType.DeleteButton.ToString()
                }
            };
        }
    }
}
