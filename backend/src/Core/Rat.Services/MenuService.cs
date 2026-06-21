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

            return BuildMenuTree(allAdminMenuItems, default(int));
        }

        /// <summary>
        /// Recursively build the menu tree for a given parent, so menu items nest to any depth
        /// (not just one level of children).
        /// </summary>
        /// <param name="allMenuItems">flat list of all menu items</param>
        /// <param name="parentMenuItemId">parent ID to build children for (0 = root)</param>
        /// <returns>ordered list of menu items with their child sub-trees populated</returns>
        private IList<MenuItemDto> BuildMenuTree(IList<MenuItem> allMenuItems, int parentMenuItemId)
        {
            var menuItems = GetChildMenuItemsByParentId(allMenuItems, parentMenuItemId);

            foreach (var menuItem in menuItems)
            {
                menuItem.ChildMenuItems.AddRange(BuildMenuTree(allMenuItems, menuItem.Id));
            }

            return menuItems;
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
