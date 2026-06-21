using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rat.Contracts.Common;

namespace Rat.Services
{
    public partial interface IEntityService
    {
        /// <summary>
        /// Get common entity by name and ID
        /// </summary>
        /// <param name="entityName">entity name</param>
        /// <param name="entityId">entity ID</param>
        /// <returns>entity entries (properities) DTO for administration</returns>
        Task<IList<EntityEntryDto>> GetEntityAsync(string entityName, int? entityId);

        /// <summary>
        /// Save common entity
        /// </summary>
        /// <param name="entityName">entity name</param>
        /// <param name="data">column names and values to save</param>
        Task SaveEntityAsync(string entityName, Dictionary<string, object> data);

        /// <summary>
        /// Delete common entity by name and entity ID
        /// </summary>
        /// <param name="entityName">entity name</param>
        /// <param name="entityId">entity ID</param>
        /// <param name="skipCommonAccessAttribute">ignore defined common access attribute</param>
        Task DeleteEntityAsync(string entityName, int entityId, bool skipCommonAccessAttribute = false);

        /// <summary>
        /// Get all common entity entries to table
        /// </summary>
        /// <param name="entityName">entity name</param>
        /// <returns>dynamic entity model</returns>
        Task<dynamic> GetAllToTableAsync(string entityName);

        /// <summary>
        /// Get entity type by name
        /// </summary>
        /// <param name="entityName">entity name</param>
        /// <returns>entity type</returns>
        Type GetTableEntityTypeByName(string entityName);
    }
}
