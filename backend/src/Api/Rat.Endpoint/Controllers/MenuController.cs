using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Rat.Services;

namespace Rat.Endpoint.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public partial class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenuController(
            IMenuService menuService)
        {
            _menuService = menuService;
        }

        /// <summary>
        /// Get administration menu
        /// </summary>
        /// <returns>list of menu items</returns>
        [HttpPost]
        public virtual async Task<IActionResult> GetMenu()
        {
            var data = await _menuService.GetAdminMenuItemsAsync();

            return Ok(data);
        }
    }
}
