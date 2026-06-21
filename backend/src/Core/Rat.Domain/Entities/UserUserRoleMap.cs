using Rat.Domain.EntityAttributes;

namespace Rat.Domain.Entities
{
    /// <summary>
    /// Represents database table
    /// </summary>
    public partial class UserUserRoleMap : TableEntity
    {
        /// <summary>
        /// Binding to specific user by ID
        /// </summary>
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        /// <summary>
        /// Binding to specific user role by ID
        /// </summary>
        [ForeignKey(nameof(UserRole))]
        public int UserRoleId { get; set; }
    }
}
