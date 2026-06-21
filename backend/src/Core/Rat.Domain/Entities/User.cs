using System;
using Rat.Domain.EntityAttributes;
using Rat.Domain.EntityTypes;

namespace Rat.Domain.Entities
{
    /// <summary>
    /// Represents database table
    /// </summary>
    [CommonAccess]
    public partial class User : TableEntity, IAuditable, ISoftDelete
    {
        public User()
        {
            UserGuid = Guid.NewGuid();
        }

        /// <summary>
        /// User GUID identifier
        /// </summary>
        public Guid UserGuid { get; set; }

        /// <summary>
        /// User email
        /// </summary>
        [NotNullableString]
        [MaxStringLength(EntityDefaults.MaxEmailLength)]
        public string Email { get; set; }

        /// <summary>
        /// System name as system user identifier
        /// </summary>
        [MaxStringLength(EntityDefaults.MaxTypicalStringLength)]
        public string SystemName { get; set; }

        /// <summary>
        /// Is user VAT payer
        /// </summary>
        public bool IsVatPayer { get; set; }

        /// <summary>
        /// Is user active (or blocked)
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// An administrator's note about a user
        /// </summary>
        public string AdminNote { get; set; }

        /// <summary>
        /// Count of invalid login attempts
        /// </summary>
        public int InvalidLoginAttempts { get; set; }

        /// <summary>
        /// Last IP address belonging to the user
        /// </summary>
        [MaxStringLength(EntityDefaults.MaxIpAddressLength)]
        public string LastIpAddress { get; set; }

        /// <summary>
        /// Last user login UTC date/time
        /// </summary>
        public DateTime? LastLoginUTC { get; set; }

        /// <summary>
        /// Date of the user creation
        /// </summary>
        public DateTime CreatedUTC { get; set; }

        /// <summary>
        /// Date of the last user modification
        /// </summary>
        public DateTime? ModifiedUTC { get; set; }

        /// <summary>
        /// Is user soft deleted
        /// </summary>
        public bool Deleted { get; set; }
    }
}
