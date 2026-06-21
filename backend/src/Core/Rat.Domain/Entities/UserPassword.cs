using System;
using Rat.Domain.EntityAttributes;

namespace Rat.Domain.Entities
{
    /// <summary>
    /// Represents database table
    /// </summary>
    public partial class UserPassword : TableEntity
    {
        /// <summary>
        /// User ID biding
        /// </summary>
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        /// <summary>
        /// Password hash
        /// </summary>
        [NotNullableString]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Password salt
        /// </summary>
        [NotNullableString]
        public string PasswordSalt { get; set; }

        /// <summary>
        /// Type of hash used
        /// </summary>
        public int HashTypeId { get; set; }

        /// <summary>
        /// Date the password was created
        /// </summary>
        public DateTime CreatedUTC { get; set; }
    }
}
