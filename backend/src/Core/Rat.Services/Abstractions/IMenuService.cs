using System.Collections.Generic;
using System.Threading.Tasks;
using Rat.Contracts.Models;

namespace Rat.Services
{
    public partial interface IMenuService
    {
        /// <summary>
        /// Get all administration menu items
        /// </summary>
        /// <returns>list of all administartion menu items</returns>
        Task<IList<MenuItemDto>> GetAdminMenuItemsAsync();
    }
}
