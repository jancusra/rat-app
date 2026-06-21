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
    /// Generic service for working with arbitrary table entities resolved by name at runtime.
    /// Provides metadata-driven CRUD and table projection (read, save, delete, list) without
    /// knowing the concrete entity type at compile time: the CLR type is looked up by name,
    /// repository methods are dispatched via reflection, and DTO/column metadata is built from
    /// the entity's properties combined with code-configured entry metadata. Also handles
    /// many-to-many mappings (MappedMultiSelect) through convention-named mapping tables.
    /// Structural reflection lookups (types, methods, properties) are cached for performance.
    /// </summary>
    public partial class EntityService : IEntityService
    {
        private readonly IAppTypeFinder _appTypeFinder;

        private readonly IRepository _repository;

        private readonly IReflectionCache _reflectionCache;

        private readonly IUserService _userService;

        private const string MetadataMethodName = "GetMetadata";

        public EntityService(
            IAppTypeFinder appTypeFinder,
            IRepository repository,
            IReflectionCache reflectionCache,
            IUserService userService)
        {
            _appTypeFinder = appTypeFinder;
            _repository = repository;
            _reflectionCache = reflectionCache;
            _userService = userService;
        }

        public virtual async Task<IList<EntityEntryDto>> GetEntityAsync(string entityName, int? entityId)
        {
            var entityType = GetTableEntityTypeByName(entityName);

            if (!entityType.HasSpecificAttribute<CommonAccessAttribute>())
            {
                return new List<EntityEntryDto>();
            }

            await EnsureAdministrationAccessAsync(entityType, AccessType.ReadOnly);

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

            await EnsureAdministrationAccessAsync(entityType, AccessType.FullAccess);

            int.TryParse(data[nameof(TableEntity.Id)]?.ToString(), out int entityId);

            var entity = await PrepareAndInsertOrUpdateEntityAsync(entityType, entityId, data);
            await SaveEntityAdditionsByMetadata(entityType, entity.Id, data);
        }

        public virtual async Task DeleteEntityAsync(string entityName, int entityId, bool skipCommonAccessAttribute = false)
        {
            var entityType = GetTableEntityTypeByName(entityName);

            if (!skipCommonAccessAttribute)
            {
                if (!entityType.HasSpecificAttribute<CommonAccessAttribute>())
                {
                    return;
                }

                await EnsureAdministrationAccessAsync(entityType, AccessType.FullAccess);
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
                return new { columns = new List<ColumnMetadata>(), data = new List<IDictionary<string, object>>() };
            }

            await EnsureAdministrationAccessAsync(entityType, AccessType.ReadOnly);

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

                            var primaryIdColumnName = $"{entityName}{nameof(TableEntity.Id)}";
                            var objectIdColumnName = $"{entryMetadata.Name}{nameof(TableEntity.Id)}";

                            // Group mapped object IDs by primary entity ID in a single pass to avoid scanning all mappings per row
                            var mapObjectIdsByPrimaryId = new Dictionary<int, HashSet<int>>();

                            foreach (var mappingEntry in mappingsData)
                            {
                                var primaryId = GetIntValueByPropertyName(mappingEntry, primaryIdColumnName);
                                var objectId = GetIntValueByPropertyName(mappingEntry, objectIdColumnName);

                                if (!mapObjectIdsByPrimaryId.TryGetValue(primaryId, out var objectIds))
                                {
                                    objectIds = new HashSet<int>();
                                    mapObjectIdsByPrimaryId[primaryId] = objectIds;
                                }

                                objectIds.Add(objectId);
                            }

                            foreach (var tableEntry in tableDictData)
                            {
                                var primaryId = (int)tableEntry[nameof(TableEntity.Id).FirstCharToLowerCase()];
                                var mapObjectIds = mapObjectIdsByPrimaryId.TryGetValue(primaryId, out var ids)
                                    ? ids : new HashSet<int>();

                                tableEntry.Add(entryMetadata.Name.FirstCharToLowerCase(),
                                    namedObjects.Where(x => mapObjectIds.Contains(x.Key)).Select(y => y.Value).ToList());
                            }

                            break;
                        }
                    default:
                        break;
                }
            }

            return new { columns = columns, data = tableDictData };
        }

        public virtual Type GetTableEntityTypeByName(string entityName)
        {
            return _reflectionCache.GetOrAddEntityType(entityName, name =>
            {
                var typeEntityData = _appTypeFinder.GetAssemblyQualifiedNameByClass(name, ClassType.Entities);

                if (string.IsNullOrEmpty(typeEntityData))
                {
                    throw new NonExistingEntityException(name);
                }

                var entityType = Type.GetType(typeEntityData);

                if (entityType == null)
                {
                    throw new NonExistingEntityException(name);
                }

                return entityType;
            });
        }

        /// <summary>
        /// Ensure the current user has at least the required administration access for the entity.
        /// Access types are ordered by permissiveness (FullAccess is the most permissive),
        /// so a lower numeric value satisfies a higher required level.
        /// </summary>
        /// <param name="entityType">the type of entity being accessed</param>
        /// <param name="requiredAccess">minimum access required for the operation</param>
        /// <exception cref="AccessDeniedException">thrown when the current user lacks sufficient access</exception>
        private async Task EnsureAdministrationAccessAsync(Type entityType, AccessType requiredAccess)
        {
            var currentAccess = await _userService.GetCurrentUserAdministrationAccessAsync();

            if (currentAccess > requiredAccess)
            {
                throw new AccessDeniedException(entityType.Name);
            }
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
            var methodToInvoke = _reflectionCache.GetOrAddMethod((typeOfSource, methodName, methodGenericType), key =>
            {
                var baseMethod = key.Source.GetMethod(key.Method);
                return key.Generic != null ? baseMethod.MakeGenericMethod(key.Generic) : baseMethod;
            });

            var invocationResult = methodToInvoke.Invoke(sourceForInvoke, invokeParameters);

            if (methodToInvoke.ReturnType == typeof(void))
            {
                return null;
            }

            dynamic awaitableData = invocationResult;

            if (methodToInvoke.ReturnType == typeof(Task))
            {
                await awaitableData;
                return null;
            }

            return await awaitableData;
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

            if (entity == null)
            {
                throw new NonExistingEntityEntryException(entityType.Name);
            }

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
            var entityProperties = GetCachedProperties(entity.GetType());

            foreach (var entityEntry in data)
            {
                if (entityEntry.Key == nameof(TableEntity.Id))
                {
                    continue;
                }

                var propertyInfo = entityProperties.FirstOrDefault(x => x.Name == entityEntry.Key);

                if (propertyInfo != null && propertyInfo.CanWrite
                    && TryConvertToPropertyType(entityEntry.Value, propertyInfo.PropertyType, out var convertedValue))
                {
                    propertyInfo.SetValue(entity, convertedValue, null);
                }
            }
        }

        /// <summary>
        /// Convert a raw data value (usually deserialized from request data) to a property type.
        /// Handles null, Nullable, enum and primitive conversions; returns false for values that
        /// cannot be converted so a single bad field does not break the whole save.
        /// </summary>
        /// <param name="value">raw value to convert</param>
        /// <param name="targetType">the property type to convert to</param>
        /// <param name="converted">the converted value when conversion succeeds</param>
        /// <returns>true when the value can be assigned to the property</returns>
        private bool TryConvertToPropertyType(object value, Type targetType, out object converted)
        {
            converted = null;
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (value == null)
            {
                // only reference types and Nullable<T> can hold null; skip non-nullable value types
                return !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null;
            }

            if (underlyingType.IsInstanceOfType(value))
            {
                converted = value;
                return true;
            }

            try
            {
                converted = underlyingType.IsEnum
                    ? (value is string enumName
                        ? Enum.Parse(underlyingType, enumName, true)
                        : Enum.ToObject(underlyingType, Convert.ToInt64(value)))
                    : Convert.ChangeType(value, underlyingType);

                return true;
            }
            catch (Exception ex) when (ex is InvalidCastException || ex is FormatException
                || ex is OverflowException || ex is ArgumentException)
            {
                return false;
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
                            var newObjectIds = data.TryGetValue(entryMetadata.Name, out var rawObjectIds) && rawObjectIds is IList<object> rawObjectIdList
                                ? rawObjectIdList.Select(Convert.ToInt32).ToList()
                                : new List<int>();
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

                            if (objectIdsToCreate.Count > default(int))
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
                        }
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
            var entityProperties = GetCachedProperties(entityType);
            var entityEntries = new List<EntityEntryDto>();

            if (entity != null && typeof(ICanBeSystem).IsAssignableFrom(entityType))
            {
                var systemEntityProperty = entityProperties.FirstOrDefault(x => x.Name == nameof(ICanBeSystem.IsSystemEntry));

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

                var entityValue = entityProperty.GetValue(entity, null);
                var entityEntryType = entityProperty.PropertyType.Name;

                if (alteredEntry != null && !string.IsNullOrEmpty(alteredEntry.EntryType))
                {
                    entityEntryType = alteredEntry.EntryType;

                    if (entityEntryType == CustomEntityEntryType.Enum.ToString())
                    {
                        var enumValue = entityValue != null ? Convert.ToInt32(entityValue) : default(int);

                        if (!alteredEntry.SelectOptions.ContainsKey(enumValue))
                        {
                            var firstEnumEntry = alteredEntry.SelectOptions.FirstOrDefault();
                            entityValue = firstEnumEntry.Key;
                        }
                    }
                }

                entityEntries.Add(new EntityEntryDto
                {
                    Name = entityProperty.Name,
                    Value = entityValue,
                    EntryType = entityEntryType,
                    Hidden = alteredEntry?.Hidden ?? false,
                    SelectOptions = alteredEntry?.SelectOptions
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

                            var index = Math.Min(entryMetadata.Order, entityEntries.Count);
                            entityEntries.Insert(index, entityEntryDto);
                            break;
                        }
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

            var entityPropNames = GetCachedProperties(entityType).Select(x => x.Name).ToList();
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

            foreach (var entityProperty in GetCachedProperties(entityType))
            {
                var alteredEntry = entriesMetadata.FirstOrDefault(x => x.Name == entityProperty.Name);
                var alteredColumn = columnsMetadata.FirstOrDefault(x => x.Name == entityProperty.Name);
                var alteredData = alteredColumn ?? alteredEntry as BaseEntryDto;

                if (alteredData != null && alteredData.Excluded)
                {
                    continue;
                }

                if (IsEntityPropertyDerivedByEntityType<IAuditable>(entityType, entityProperty))
                {
                    continue;
                }

                columns.Add(new ColumnMetadata
                {
                    Name = entityProperty.Name,
                    EntryType = alteredData != null && !string.IsNullOrEmpty(alteredData.EntryType)
                        ? alteredData.EntryType : entityProperty.PropertyType.Name,
                    Hidden = alteredData?.Hidden ?? false,
                    SelectOptions = alteredData?.SelectOptions
                });
            }

            foreach (var entryMetadata in expandingMetadata)
            {
                var alteredColumn = columnsMetadata.FirstOrDefault(x => x.Name == entryMetadata.Name);
                var alteredEntry = entriesMetadata.FirstOrDefault(x => x.Name == entryMetadata.Name);
                var alteredData = alteredColumn ?? alteredEntry as BaseEntryDto;

                if (alteredData != null && alteredData.Excluded)
                {
                    continue;
                }

                var expandingColumn = new ColumnMetadata
                {
                    Name = entryMetadata.Name,
                    EntryType = alteredData != null && !string.IsNullOrEmpty(alteredData.EntryType)
                        ? alteredData.EntryType : entryMetadata.EntryType,
                    Hidden = alteredData?.Hidden ?? false,
                    SelectOptions = alteredData?.SelectOptions
                };

                switch (entryMetadata.EntryType.ToEnum<CustomEntityEntryType>())
                {
                    case CustomEntityEntryType.MappedMultiSelect:
                        {
                            expandingColumn.SelectOptions = await GetAllNamedByEntityNameAsync(entryMetadata.Name);
                            break;
                        }
                    default:
                        break;
                }

                var index = Math.Min(alteredData != null ? alteredData.Order : entryMetadata.Order, columns.Count);
                columns.Insert(index, expandingColumn);
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
                && GetCachedProperties(typeof(T)).FirstOrDefault(x => x.Name == entityProperty.Name) != null;
        }

        /// <summary>
        /// Get (and cache) the public properties of a type
        /// </summary>
        /// <param name="type">the type to reflect</param>
        /// <returns>the type's public properties</returns>
        private PropertyInfo[] GetCachedProperties(Type type)
        {
            return _reflectionCache.GetOrAddProperties(type, t => t.GetProperties());
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
            var metadataMethod = _reflectionCache.GetOrAddMetadataMethod((entityType.Name, classType), key =>
            {
                var metadataType = _appTypeFinder.GetAssemblyQualifiedNameByClass(key.EntityName, key.ClassType);

                if (string.IsNullOrEmpty(metadataType))
                {
                    return null;
                }

                return Type.GetType(metadataType)?.GetMethod(MetadataMethodName);
            });

            if (metadataMethod != null)
            {
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
            var property = source.GetType().GetProperty(propertyName);

            if (property == null)
            {
                throw new NonExistingEntityEntryException(propertyName);
            }

            return (int)property.GetValue(source, null);
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
