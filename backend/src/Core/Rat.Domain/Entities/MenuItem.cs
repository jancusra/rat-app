using Rat.Domain.EntityAttributes;

namespace Rat.Domain.Entities
{
    /// <summary>
    /// Represents database table
    /// </summary>
    public partial class MenuItem : TableEntity
    {
        /// <summary>
        /// Type of menu entry
        /// </summary>
        public int MenuTypeId { get; set; }

        /// <summary>
        /// Menu item system name identifier
        /// </summary>
        [NotNullableString]
        [MaxStringLength(EntityDefaults.MaxTypicalStringLength)]
        public string SystemName { get; set; }

        /// <summary>
        /// Menu item URL address for a content
        /// </summary>
        [MaxStringLength(EntityDefaults.MaxTypicalStringLength)]
        public string Url { get; set; }

        /// <summary>
        /// Menu item icon
        /// </summary>
        [MaxStringLength(EntityDefaults.MaxTypicalStringLength)]
        public string Icon { get; set; }

        /// <summary>
        /// Menu item order
        /// </summary>
        public int ItemOrder { get; set; }

        /// <summary>
        /// Link to parent menu item entry
        /// </summary>
        public int ParentMenuItemId { get; set; }
    }
}
