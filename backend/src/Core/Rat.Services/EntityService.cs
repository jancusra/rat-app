using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Rat.Contracts.Common;
using Rat.Domain;
using Rat.Domain.EntityAttributes;
using Rat.Domain.EntityTypes;
using Rat.Domain.Exceptions;
using Rat.Domain.Extensions;
using Rat.Domain.Infrastructure;
using Rat.Domain.Types;

namespace Rat.Services
{
    /// <summary>
    /// Methods for various operations with common entities
    /// </summary>
    public partial class EntityService : IEntityService
    {
        private readonly IAppTypeFinder _appTypeFinder;

        private readonly IRepository _repository;

        public EntityService(
            IAppTypeFinder appTypeFinder,
            IRepository repository)
        {
            _appTypeFinder = appTypeFinder;
            _repository = repository;
        }

        public virtual async Task<IList<EntityEntryDto>> GetEntityAsync(string entityName, int? entityId)
        {
            var entityType = GetTableEntityTypeByName(entityName);

            if (!entityType.HasSpecificAttribute<CommonAccessAttribute>())
            {
                return new List<EntityEntryDto>();
            }

            if (!entityId.HasValue || entityId == default(int))
            {
                return await PrepareEntityEntriesDtoByEntityAsync(entityType);
            }
            else
            {
                var entity = await GetEntityByIdAsync(entityType, entityId.Value);
                return await PrepareEntityEntriesDtoByEntityAsync(entityType, entity);
            }
        }

        public virtual async Task SaveEntityAsync(string entityName, Dictionary<string, object> data)
        {
            var entityType = GetTableEntityTypeByName(entityName);

            if (!entityType.HasSpecificAttribute<CommonAccessAttribute>() || !data.ContainsKey(nameof(TableEntity.Id)))
            {
                return;
            }

            int.TryParse(data[nameof(TableEntity.Id)].ToString(), out int entityId);

            var entity = await PrepareAndInsertOrUpdateEntityAsync(entityType, entityId, data);
            await SaveEntityAdditionsByMetadata(entityType, entity.Id, data);
        }

        public virtual async Task DeleteEntityAsync(string entityName, int entityId, bool skipCommonAccessAttribute = false)
        {
            var entityType = GetTableEntityTypeByName(entityName);

            if (!skipCommonAccessAttribute && !entityType.HasSpecificAttribute<CommonAccessAttribute>())
            {
                return;
            }

            await GetResultFromInvokedMethodAsync(
                typeof(IRepository),
                nameof(IRepository.DeleteAsync),
                _repository,
                new object[] { entityId },
                entityType);
        }

        public virtual async Task<dynamic> GetAllToTableAsync(string entityName)
        {
            var entityType = GetTableEntityTypeByName(entityName);

            if (!entityType.HasSpecificAttribute<CommonAccessAttribute>())
            {
                return new { columns = new List<ColumnMetadata>(), data = new int[0] };
            }

            var columns = await PrepareColumnsMetadataByEntityAsync(entityType);
            var tableData = await GetAllEntitiesAsync<dynamic>(entityType);
            var tableDictData = ConvertDynamicDataToDictionary(tableData);
            var expandingMetadata = GetExpandingMetadataByEntityType(entityType);

            foreach (var entryMetadata in expandingMetadata)
            {
                switch (entryMetadata.EntryType.ToEnum<CustomEntityEntryType>())
                {
                    case CustomEntityEntryType.MappedMultiSelect:
                        {
                            var mapEntityType = GetTableEntityTypeByName(GetMappingTableName(entityName, entryMetadata.Name));
                            var mappingsData = await GetAllEntitiesAsync<TableEntity>(mapEntityType);
                            var namedObjects = await GetAllNamedByEntityNameAsync(entryMetadata.Name);

                            foreach (var tableEntry in tableDictData)
                            {
                                var mapObjectIds = await GetMapObjectIdsByEntityNamesAndPrimaryEntityIdAsync(
                                    entityName, entryMetadata.Name, (int)tableEntry[nameof(TableEntity.Id).ToLower()], mappingsData);

                                tableEntry.Add(entryMetadata.Name.FirstCharToLowerCase(), 
                                    namedObjects.Where(x => mapObjectIds.Contains(x.Key)).Select(y => y.Value).ToList());
                            }

                            break;
                        };
                    default:
                        break;
                }
            }

            return new { columns = columns, data = tableDictData };
        }

        public virtual Type GetTableEntityTypeByName(string entityName)
        {
            var typeEntityData = _appTypeFinder.GetAssemblyQualifiedNameByClass(entityName, ClassType.Entities);

            if (string.IsNullOrEmpty(typeEntityData))
            {
                throw new NonExistingEntityException(entityName);
            }

            return Type.GetType(typeEntityData);
        }

        /// <summary>
        /// Get result from invoked method
        /// </summary>
        /// <param name="typeOfSource">the type of source (IRepository)</param>
        /// <param name="methodName">repository method name</param>
        /// <param name="sourceForInvoke">invoke source (_repository)</param>
        /// <param name="invokeParameters">parameters for invoked method</param>
        /// <param name="methodGenericType">generic type for the invoked method</param>
        /// <returns>model returned from invoked method</returns>
        private async Task<dynamic> GetResultFromInvokedMethodAsync(
            Type typeOfSource,
            string methodName,
            object sourceForInvoke,
            object[] invokeParameters,
            Type methodGenericType = null)
        {
            var baseMethod = typeOfSource.GetMethod(methodName);
            MethodInfo methodToInvoke;

            if (methodGenericType != null)
            {
                methodToInvoke = baseMethod.MakeGenericMethod(methodGenericType);
            }
            else
            {
                methodToInvoke = baseMethod;
            }

            dynamic awaitableData = methodToInvoke.Invoke(sourceForInvoke, invokeParameters);
            await awaitableData;

            if (methodToInvoke.ReturnType == typeof(void) || methodToInvoke.ReturnType == typeof(Task))
            {
                return null;
            }
            else
            {
                return awaitableData.GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Get entity by ID (by calling invoked method)
        /// </summary>
        /// <param name="entityType">entity type</param>
        /// <param name="entityId">entity ID</param>
        /// <returns>final table entity</returns>
        private async Task<TableEntity> GetEntityByIdAsync(Type entityType, int entityId)
        {
            return await GetResultFromInvokedMethodAsync(
                typeof(IRepository),
                nameof(IRepository.GetByIdAsync),
                _repository,
                new object[] { entityId },
                entityType) as TableEntity;
        }

        /// <summary>
        /// Get all entities by type (by calling invoked method)
        /// </summary>
        /// <typeparam name="T">the type of returned entities</typeparam>
        /// <param name="entityType">the type of entity</param>
        /// <returns>enumerable of all entities</returns>
        private async Task<IEnumerable<T>> GetAllEntitiesAsync<T>(Type entityType)
        {
            return await GetResultFromInvokedMethodAsync(
                typeof(IRepository),
                nameof(IRepository.GetAllAsync),
                _repository,
                null,
                entityType) as IEnumerable<T>;
        }

        /// <summary>
        /// Insert or update entity in the database (by calling invoked method)
        /// </summary>
        /// <param name="entityType">type of the entity</param>
        /// <param name="entityId">entity ID (if zero perform insert operation)</param>
        /// <param name="data">entity data (column names and values)</param>
        /// <returns>final inserted/updated entity</returns>
        private async Task<TableEntity> PrepareAndInsertOrUpdateEntityAsync(Type entityType, int entityId, Dictionary<string, object> data)
        {
            var methodName = entityId > default(int) ? nameof(IRepository.UpdateAsync) : nameof(IRepository.InsertAsync);
            var entity = entityId > default(int) ?
                await GetEntityByIdAsync(entityType, entityId) :
                Activator.CreateInstance(entityType) as TableEntity;

            SetEntityPropertiesByData(entity, data);

            await GetResultFromInvokedMethodAsync(
                typeof(IRepository),
                methodName,
                _repository,
                new object[] { entity },
                entityType);

            return entity;
        }

        /// <summary>
        /// Set entity properties by data dictionary
        /// </summary>
        /// <param name="entity">prepared entity</param>
        /// <param name="data">entity data to set</param>
        private void SetEntityPropertiesByData(TableEntity entity, Dictionary<string, object> data)
        {
            var entityProperties = entity.GetType().GetProperties();

            foreach (var entityEntry in data)
            {
                if (entityEntry.Key != nameof(TableEntity.Id))
                {
                    var propertyInfo = entityProperties.FirstOrDefault(x => x.Name == entityEntry.Key);

                    if (propertyInfo != null)
                    {
                        propertyInfo.SetValue(entity, entityEntry.Value, null);
                    }
                }
            }
        }

        /// <summary>
        /// Save additional records for the entity according to the configured metadata
        /// </summary>
        /// <param name="entityType">the type of entity</param>
        /// <param name="entityId">entity ID</param>
        /// <param name="data">entity data</param>
        private async Task SaveEntityAdditionsByMetadata(Type entityType, int entityId, Dictionary<string, object> data)
        {
            var expandingMetadata = GetExpandingMetadataByEntityType(entityType);

            foreach (var entryMetadata in expandingMetadata)
            {
                switch (entryMetadata.EntryType.ToEnum<CustomEntityEntryType>())
                {
                    case CustomEntityEntryType.MappedMultiSelect:
                        {
                            var newObjectIds = ((IList<object>)data[entryMetadata.Name]).Cast<int>().ToList();
                            var objectIdColumnName = $"{entryMetadata.Name}{nameof(TableEntity.Id)}";
                            var entityMaps = await GetMapsByEntityNamesAndPrimaryEntityIdAsync(
                                    entityType.Name, entryMetadata.Name, entityId);
                            var savedObjectIds = new List<int>();
                            var mapIdsToDelete = new List<int>();

                            foreach (var entityMap in entityMaps)
                            {
                                var objectId = GetIntValueByPropertyName(entityMap, objectIdColumnName);
                                savedObjectIds.Add(objectId);

                                if (!newObjectIds.Contains(objectId))
                                {
                                    mapIdsToDelete.Add(entityMap.Id);
                                }
                            }

                            var objectIdsToCreate = newObjectIds.Except(savedObjectIds).ToList();
                            var mappingTableName = GetMappingTableName(entityType.Name, entryMetadata.Name);

                            if (objectIdsToCreate.Count() > default(int))
                            {
                                var mapEntityType = GetTableEntityTypeByName(mappingTableName);

                                foreach (var objectIdToCreate in objectIdsToCreate)
                                {
                                    var newData = new Dictionary<string, object>()
                                        {
                                            { $"{entityType.Name}{nameof(TableEntity.Id)}", entityId },
                                            { $"{entryMetadata.Name}{nameof(TableEntity.Id)}", objectIdToCreate }
                                        };

                                    await PrepareAndInsertOrUpdateEntityAsync(mapEntityType, default(int), newData);
                                }
                            }

                            foreach (var mapIdToDelete in mapIdsToDelete)
                            {
                                await DeleteEntityAsync(mappingTableName, mapIdToDelete, true);
                            }

                            break;
                        };
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Prepare entity DTO model by raw table entity
        /// </summary>
        /// <param name="entityType">the type of entity</param>
        /// <param name="entity">existing table entity (prepare new if not defined)</param>
        /// <returns>entity properties DTO model</returns>
        private async Task<IList<EntityEntryDto>> PrepareEntityEntriesDtoByEntityAsync(Type entityType, TableEntity entity = null)
        {
            var entriesMetadata = GetMetadataByEntityAndClassType<EntityEntryDto>(entityType, ClassType.CommonEntityEntries);
            var expandingMetadata = GetExpandingMetadataByEntityType(entityType, entriesMetadata);
            var entityProperties = entityType.GetProperties();
            var entityEntries = new List<EntityEntryDto>();

            if (entity != null && typeof(ICanBeSystem).IsAssignableFrom(entityType))
            {
                var systemEntityProperty = entityType.GetProperties().FirstOrDefault(x => x.Name == nameof(ICanBeSystem.IsSystemEntry));

                if (systemEntityProperty != null && (bool)systemEntityProperty.GetValue(entity, null))
                {
                    entity = null;
                }
            }

            if (entity == null)
            {
                entity = Activator.CreateInstance(entityType) as TableEntity;
            }

            foreach (var entityProperty in entityProperties)
            {
                var alteredEntry = entriesMetadata.FirstOrDefault(x => x.Name == entityProperty.Name);

                if (alteredEntry != null && alteredEntry.Excluded)
                {
                    continue;
                }

                if (IsEntityPropertyDerivedByEntityType<IAuditable>(entityType, entityProperty)
                    || IsEntityPropertyDerivedByEntityType<ICanBeSystem>(entityType, entityProperty))
                {
                    continue;
                }

                var entityValue = entity != null ? entityProperty.GetValue(entity, null) : null;
                var entityEntryType = entityProperty.PropertyType.Name;

                if (alteredEntry != null && !string.IsNullOrEmpty(alteredEntry.EntryType))
                {
                    entityEntryType = alteredEntry.EntryType;

                    if (entityEntryType == CustomEntityEntryType.Enum.ToString() 
                        && !alteredEntry.SelectOptions.ContainsKey((int)entityValue))
                    {
                        var firstEnumEntry = alteredEntry.SelectOptions.FirstOrDefault();
                        entityValue = firstEnumEntry.Key;
                    }
                }

                entityEntries.Add(new EntityEntryDto
                {
                    Name = entityProperty.Name,
                    Value = entityValue,
                    EntryType = entityEntryType,
                    Hidden = alteredEntry != null ? alteredEntry.Hidden : false,
                    SelectOptions = alteredEntry != null ? alteredEntry.SelectOptions : null
                });
            }

            foreach (var entryMetadata in expandingMetadata)
            {
                switch (entryMetadata.EntryType.ToEnum<CustomEntityEntryType>())
                {
                    case CustomEntityEntryType.MappedMultiSelect:
                        {
                            var entityEntryDto = new EntityEntryDto
                            {
                                Name = entryMetadata.Name,
                                EntryType = entryMetadata.EntryType,
                                Value = entity.Id > default(int) 
                                    ? await GetMapObjectIdsByEntityNamesAndPrimaryEntityIdAsync(entityType.Name, entryMetadata.Name, entity.Id)
                                    : new List<int>(),
                                SelectOptions = await GetAllNamedByEntityNameAsync(entryMetadata.Name)
                            };

                            entityEntries.Insert(entryMetadata.Order, entityEntryDto);
                            break;
                        };
                    default:
                        break;
                }
            }

            return entityEntries;
        }

        /// <summary>
        /// Extending the DTO properties of a entity with other properties configured in the code
        /// </summary>
        /// <param name="entityType">the type of entity</param>
        /// <param name="entriesMetadata">entity entries metadata</param>
        /// <returns>extended DTO properties</returns>
        private IList<EntityEntryDto> GetExpandingMetadataByEntityType(
            Type entityType, 
            IList<EntityEntryDto> entriesMetadata = null)
        {
            if (entriesMetadata is null)
            {
                entriesMetadata = GetMetadataByEntityAndClassType<EntityEntryDto>(entityType, ClassType.CommonEntityEntries);
            }

            var entityPropNames = entityType.GetProperties().Select(x => x.Name).ToList();
            return entriesMetadata.Where(x => !entityPropNames.Contains(x.Name)).ToList();
        }

        /// <summary>
        /// Get all named entries by entity name
        /// </summary>
        /// <param name="entityName">entity name</param>
        /// <returns>dictionary: entity ID and corresponding entity name</returns>
        private async Task<Dictionary<int, string>> GetAllNamedByEntityNameAsync(string entityName)
        {
            var optionsEntityType = GetTableEntityTypeByName(entityName);
            var optionsData = await GetAllEntitiesAsync<TableEntity>(optionsEntityType);
            var namedEntries = new Dictionary<int, string>();

            foreach (var optionEntry in optionsData)
            {
                if (typeof(INamed).IsAssignableFrom(optionEntry.GetType()))
                {
                    namedEntries.Add(optionEntry.Id, (optionEntry as INamed).Name);
                }
            }

            return namedEntries;
        }

        /// <summary>
        /// Get mapping table by primary/secondary table names
        /// </summary>
        /// <param name="primaryEntityName">primary entity name</param>
        /// <param name="secondaryEntityName">secondary entity name</param>
        /// <returns>mapping table name</returns>
        private string GetMappingTableName(string primaryEntityName, string secondaryEntityName)
        {
            return $"{primaryEntityName}{secondaryEntityName}{EntityDefaults.MappingTableNamePostfix}";
        }

        /// <summary>
        /// Get mapped object IDs by primary/secondary entity and primary entity ID
        /// </summary>
        /// <param name="primaryEntityName">primary entity name</param>
        /// <param name="secondaryEntityName">secondary entity name</param>
        /// <param name="primaryEntityId">primary entity ID</param>
        /// <param name="mappingsData">existing mappings data (used as source if defined)</param>
        /// <returns>list of map IDs</returns>
        private async Task<List<int>> GetMapObjectIdsByEntityNamesAndPrimaryEntityIdAsync(
            string primaryEntityName,
            string secondaryEntityName,
            int primaryEntityId,
            IEnumerable<TableEntity> mappingsData = null)
        {
            var objectIdColumnName = $"{secondaryEntityName}{nameof(TableEntity.Id)}";
            var entityMaps = await GetMapsByEntityNamesAndPrimaryEntityIdAsync(
                primaryEntityName, secondaryEntityName, primaryEntityId, mappingsData);

            return entityMaps.Select(x => GetIntValueByPropertyName(x, objectIdColumnName)).ToList();
        }

        /// <summary>
        /// Get mapped entities by primary/secondary entity and primary entity ID
        /// </summary>
        /// <param name="primaryEntityName">primary entity name</param>
        /// <param name="secondaryEntityName">secondary entity name</param>
        /// <param name="primaryEntityId">primary entity ID</param>
        /// <param name="mappingsData">existing mappings data (used as source if defined)</param>
        /// <returns>list of mapped entities</returns>
        private async Task<IList<TableEntity>> GetMapsByEntityNamesAndPrimaryEntityIdAsync(
            string primaryEntityName,
            string secondaryEntityName,
            int primaryEntityId,
            IEnumerable<TableEntity> mappingsData = null)
        {
            var primaryIdColumnName = $"{primaryEntityName}{nameof(TableEntity.Id)}";
            var finalMaps = new List<TableEntity>();

            if (mappingsData is null)
            {
                var mapEntityType = GetTableEntityTypeByName(GetMappingTableName(primaryEntityName, secondaryEntityName));
                mappingsData = await GetAllEntitiesAsync<TableEntity>(mapEntityType);
            }

            foreach (var mappingEntry in mappingsData)
            {
                if (primaryEntityId == GetIntValueByPropertyName(mappingEntry, primaryIdColumnName))
                {
                    finalMaps.Add(mappingEntry);
                }
            }

            return finalMaps;
        }

        /// <summary>
        /// Prepare table columns by metadata configured in the code
        /// </summary>
        /// <param name="entityType">the type of entity</param>
        /// <returns>final list of column metadata</returns>
        private async Task<IList<ColumnMetadata>> PrepareColumnsMetadataByEntityAsync(Type entityType)
        {
            var entriesMetadata = GetMetadataByEntityAndClassType<EntityEntryDto>(entityType, ClassType.CommonEntityEntries);
            var columnsMetadata = GetMetadataByEntityAndClassType<ColumnMetadata>(entityType, ClassType.CommonTableColumns);
            var expandingMetadata = GetExpandingMetadataByEntityType(entityType, entriesMetadata);
            var columns = new List<ColumnMetadata>();

            foreach (var entityProperty in entityType.GetProperties())
            {
                var alteredEntry = entriesMetadata.FirstOrDefault(x => x.Name == entityProperty.Name);
                var alteredColumn = columnsMetadata.FirstOrDefault(x => x.Name == entityProperty.Name);
                var alteredData = alteredColumn != null ? alteredColumn : alteredEntry as BaseEntryDto;

                if (alteredData != null && alteredData.Excluded)
                {
                    continue;
                }

                if (IsEntityPropertyDerivedByEntityType<IAuditable>(entityType, entityProperty))
                {
                    continue;
                }

                columns.Add(new ColumnMetadata {
                    Name = entityProperty.Name,
                    EntryType = alteredData != null && !string.IsNullOrEmpty(alteredData.EntryType)
                        ? alteredData.EntryType : entityProperty.PropertyType.Name,
                    Hidden = alteredData != null ? alteredData.Hidden : false,
                    SelectOptions = alteredData != null ? alteredData.SelectOptions : null
                });
            }

            foreach (var entryMetadata in expandingMetadata)
            {
                var alteredColumn = columnsMetadata.FirstOrDefault(x => x.Name == entryMetadata.Name);
                var alteredData = alteredColumn != null ? alteredColumn : entriesMetadata as BaseEntryDto;

                if (alteredData != null && alteredData.Excluded)
                {
                    continue;
                }

                var expandingColumn = new ColumnMetadata
                {
                    Name = entryMetadata.Name,
                    EntryType = alteredData != null && !string.IsNullOrEmpty(alteredData.EntryType)
                        ? alteredData.EntryType : entryMetadata.EntryType,
                    Hidden = alteredData != null ? alteredData.Hidden : false,
                    SelectOptions = alteredData != null ? alteredData.SelectOptions : null
                };

                switch (entryMetadata.EntryType.ToEnum<CustomEntityEntryType>())
                {
                    case CustomEntityEntryType.MappedMultiSelect:
                        {
                            expandingColumn.SelectOptions = await GetAllNamedByEntityNameAsync(entryMetadata.Name);
                            break;
                        };
                    default:
                        break;
                }

                columns.Insert(alteredData != null ? alteredData.Order : entryMetadata.Order, expandingColumn);
            }

            foreach (var columnMetadata in columnsMetadata)
            {
                var column = columns.FirstOrDefault(x => x.Name == columnMetadata.Name);

                if (column == null)
                {
                    columns.Add(columnMetadata);
                }
            }

            return columns;
        }

        /// <summary>
        /// Determine whether an entity type (interface) is derived from a specific entity type and his property
        /// </summary>
        /// <typeparam name="T">entity interface type (e.g. IAuditable, INamed etc.)</typeparam>
        /// <param name="entityType">the type of entity</param>
        /// <param name="entityProperty">specific entity property</param>
        /// <returns>the bool result</returns>
        private bool IsEntityPropertyDerivedByEntityType<T>(Type entityType, PropertyInfo entityProperty)
        {
            return typeof(T).IsAssignableFrom(entityType)
                && typeof(T).GetProperties().FirstOrDefault(x => x.Name == entityProperty.Name) != null;
        }

        /// <summary>
        /// Get configured metadata by entity and class type
        /// </summary>
        /// <typeparam name="T">final metadata format</typeparam>
        /// <param name="entityType">the type of entity</param>
        /// <param name="classType">the type of class</param>
        /// <returns>final list of metadata</returns>
        private IList<T> GetMetadataByEntityAndClassType<T>(Type entityType, ClassType classType) where T : BaseEntryDto
        {
            var metadataType = _appTypeFinder.GetAssemblyQualifiedNameByClass(entityType.Name, classType);

            if (!string.IsNullOrEmpty(metadataType))
            {
                var metadataMethod = Type.GetType(metadataType).GetMethod("GetMetadata");
                return (List<T>)metadataMethod.Invoke(null, null);
            }

            return new List<T>();
        }

        /// <summary>
        /// Extract int value form property object
        /// </summary>
        /// <param name="source">input object</param>
        /// <param name="propertyName">property name</param>
        /// <returns>extracted int value</returns>
        private int GetIntValueByPropertyName(object source, string propertyName)
        {
            return (int)source.GetType().GetProperty(propertyName).GetValue(source, null);
        }

        /// <summary>
        /// Convert enumerable dynamic data to dictionary string/object
        /// </summary>
        /// <param name="dynamicData">dynamic data to convert</param>
        /// <returns>final dictionary string (property name)/object</returns>
        private IList<IDictionary<string, object>> ConvertDynamicDataToDictionary(IEnumerable<dynamic> dynamicData)
        {
            var dictionaryData = new List<IDictionary<string, object>>();

            foreach (dynamic dynamicObject in dynamicData)
            {
                var dictionary = new Dictionary<string, object>();

                foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(dynamicObject))
                {
                    object obj = propertyDescriptor.GetValue(dynamicObject);
                    dictionary.Add(propertyDescriptor.Name.FirstCharToLowerCase(), obj);
                }

                dictionaryData.Add(dictionary);
            }

            return dictionaryData;
        }
    }
}
