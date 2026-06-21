using Rat.Domain.EntityAttributes;

namespace Rat.Domain.Entities
{
    /// <summary>
    /// Represents database table
    /// </summary>
    public partial class UserRoleException : TableEntity
    {
        /// <summary>
        /// Binding to specific role by ID
        /// </summary>
        [ForeignKey(nameof(UserRole))]
        public int UserRoleId { get; set; }

        /// <summary>
        /// Binding to specific menu item by ID
        /// </summary>
        [ForeignKey(nameof(MenuItem))]
        public int MenuItemId { get; set; }

        /// <summary>
        /// Default role access type (full, read only, etc.)
        /// </summary>
        public int AccessTypeId { get; set; }
    }
}
