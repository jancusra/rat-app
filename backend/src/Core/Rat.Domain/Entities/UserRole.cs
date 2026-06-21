using System;
using Rat.Domain.EntityAttributes;
using Rat.Domain.EntityTypes;

namespace Rat.Domain.Entities
{
    /// <summary>
    /// Represents database table
    /// </summary>
    [CommonAccess]
    public partial class UserRole : TableEntity, IAuditable, ICanBeSystem, INamed
    {
        /// <summary>
        /// Name of the user role
        /// </summary>
        [NotNullableString]
        [MaxStringLength(EntityDefaults.MaxTypicalStringLength)]
        public string Name { get; set; }

        /// <summary>
        /// Default role access type (full, read only, etc.)
        /// </summary>
        public int DefaultAccessTypeId { get; set; }

        /// <summary>
        /// Is user role active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Is classified as system role
        /// </summary>
        public bool IsSystemEntry { get; set; }

        /// <summary>
        /// Date of the role creation
        /// </summary>
        public DateTime CreatedUTC { get; set; }

        /// <summary>
        /// Date of the role modification
        /// </summary>
        public DateTime? ModifiedUTC { get; set; }
    }
}
