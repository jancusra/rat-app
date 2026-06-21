using System.Collections.Generic;
using Rat.Contracts.Common;

namespace Rat.Contracts.Models
{
    /// <summary>
    /// Represents the model for the administration menu item
    /// </summary>
    public partial class MenuItemDto : BaseDto
    {
        public MenuItemDto()
        {
            ChildMenuItems = new List<MenuItemDto>();
        }

        /// <summary>
        /// Menu item title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Menu item URL address
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Menu item icon ID
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Menu item childs
        /// </summary>
        public List<MenuItemDto> ChildMenuItems { get; set; }
    }
}
