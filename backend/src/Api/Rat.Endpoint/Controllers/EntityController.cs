using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Rat.Contracts.Models.Entity;
using Rat.Services;

namespace Rat.Endpoint.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public partial class EntityController : ControllerBase
    {
        private readonly IEntityService _entityService;

        private readonly IEntityValidationService _entityValidationService;

        public EntityController(
            IEntityService entityService,
            IEntityValidationService entityValidationService)
        {
            _entityService = entityService;
            _entityValidationService = entityValidationService;
        }

        /// <summary>
        /// Get common entity by ID and entity name
        /// </summary>
        /// <param name="model">model: ID (if null will return default entity), entity name</param>
        /// <returns>final entity result</returns>
        [HttpPost]
        public virtual async Task<IActionResult> GetEntity([FromBody]GetEntityDto model)
        {
            var data = await _entityService.GetEntityAsync(model.EntityName, model.Id);
            return Ok(data);
        }

        /// <summary>
        /// Validate and save common entity
        /// </summary>
        /// <param name="model">model: entity name, language and entity data</param>
        /// <returns>validation errors or OK result</returns>
        [HttpPost]
        public virtual async Task<IActionResult> SaveEntity([FromBody]SaveEntityDto model)
        {
            var validationResult = await _entityValidationService.ValidateCommonEntityAsync(
                model.EntityName, model.Data, model.LanguageId);

            if (validationResult.Count > default(int))
            {
                return Ok(new { errors = validationResult });
            }
            else
            {
                await _entityService.SaveEntityAsync(model.EntityName, model.Data);
                return Ok(true);
            }
        }

        /// <summary>
        /// Delete common entity by ID and entity name
        /// </summary>
        /// <param name="model">model: entity ID and name</param>
        /// <returns>OK result</returns>
        [HttpPost]
        public virtual async Task<IActionResult> DeleteEntity([FromBody]DeleteEntityDto model)
        {
            await _entityService.DeleteEntityAsync(model.EntityName, model.Id);
            return Ok(true);
        }


        /// <summary>
        /// Get all common entity entries into a table
        /// </summary>
        /// <param name="model">model: entity name</param>
        /// <returns>OK result</returns>
        [HttpPost]
        public virtual async Task<IActionResult> GetAllToTable([FromBody]GetAllDto model)
        {
            var data = await _entityService.GetAllToTableAsync(model.EntityName);
            return Ok(data);
        }
    }
}
