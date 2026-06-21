using Rat.Contracts.Models;
using Rat.Domain.Entities;

namespace Rat.Mappings
{
    public static class MenuItemMap
    {
        /// <summary>
        /// Conversion of MenuItem entity to DTO model
        /// </summary>
        /// <param name="menuItem">MenuItem entity</param>
        /// <param name="translatedTitle">translated menu item title</param>
        /// <returns>final DTO model</returns>
        public static MenuItemDto ToDtoModel(this MenuItem menuItem, string translatedTitle = "")
        {
            return new MenuItemDto
            {
                Id = menuItem.Id,
                Title = translatedTitle,
                Url = menuItem.Url,
                Icon = menuItem.Icon
            };
        }
    }
}
