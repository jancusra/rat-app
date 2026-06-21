using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using Rat.Contracts.Models;
using Rat.Domain;
using Rat.Domain.Entities;
using Rat.Domain.Types;
using Rat.Mappings;

namespace Rat.Services
{
    /// <summary>
    /// Methods working with menu item entity and other features
    /// </summary>
    public partial class MenuService : IMenuService
    {
        private readonly IRepository _repository;

        public MenuService(
            IRepository repository)
        {
            _repository = repository;
        }

        public virtual async Task<IList<MenuItemDto>> GetAdminMenuItemsAsync()
        {
            var allAdminMenuItems = await GetAllMenuItemsByTypeAsync(MenuType.Admin);
            var result = new List<MenuItemDto>();

            result.AddRange(GetChildMenuItemsByParentId(allAdminMenuItems, default(int)));

            foreach (var menuItem in result)
            {
                menuItem.ChildMenuItems.AddRange(GetChildMenuItemsByParentId(allAdminMenuItems, menuItem.Id));
            }

            return result;
        }

        /// <summary>
        /// Get all menu items by specific type
        /// </summary>
        /// <param name="menuType">the type of menu</param>
        /// <returns>list of all menu items belongs to the type</returns>
        private async Task<IList<MenuItem>> GetAllMenuItemsByTypeAsync(MenuType menuType)
            => await _repository.Table<MenuItem>().Where(x => x.MenuTypeId == (int)menuType).ToListAsync();

        /// <summary>
        /// Get child menu items by parent
        /// </summary>
        /// <param name="allMenuItems">list of all menu items</param>
        /// <param name="parentMenuItemId">specific parent menu item ID to filter</param>
        /// <returns>list of all child menu items</returns>
        private IList<MenuItemDto> GetChildMenuItemsByParentId(
            IList<MenuItem> allMenuItems,
            int parentMenuItemId)
        {
            return allMenuItems.Where(x => x.ParentMenuItemId == parentMenuItemId)
                .OrderBy(x => x.ItemOrder)
                .Select(x => x.ToDtoModel(x.SystemName)).ToList();
        }
    }
}
