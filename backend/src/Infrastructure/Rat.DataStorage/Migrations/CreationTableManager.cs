using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using FluentMigrator.Builders.Alter;
using FluentMigrator.Builders.Alter.Table;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Builders.IfDatabase;
using FluentMigrator.Builders.Schema;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using Rat.Domain;
using Rat.Domain.EntityAttributes;
using Rat.Domain.Infrastructure;

namespace Rat.DataStorage.Migrations
{
    /// <summary>
    /// Class to define table migration creation/alternation
    /// </summary>
    public partial class CreationTableManager : ICreationTableManager
    {
        private readonly IAppTypeFinder _appTypeFinder;
        private readonly ISchemaExpressionRoot _schemaExpressionRoot;
        private readonly IMigrationContext _migrationContext;
        private readonly IAlterationTableManager _alterionTableManager;

        private readonly Dictionary<Type, Action<ICreateTableColumnAsTypeSyntax>> _createTypeMappings;

        public CreationTableManager(
            IAppTypeFinder appTypeFinder,
            ISchemaExpressionRoot schemaExpressionRoot,
            IMigrationContext migrationContext,
            IAlterationTableManager alterionTableManager)
        {
            _appTypeFinder = appTypeFinder;
            _schemaExpressionRoot = schemaExpressionRoot;
            _migrationContext = migrationContext;
            _alterionTableManager = alterionTableManager;

            _createTypeMappings = new Dictionary<Type, Action<ICreateTableColumnAsTypeSyntax>>
            {
                [typeof(bool)] = c => c.AsBoolean().NotNullable(),
                [typeof(bool?)] = c => c.AsBoolean().Nullable(),
                [typeof(int)] = c => c.AsInt32().NotNullable(),
                [typeof(int?)] = c => c.AsInt32().Nullable(),
                [typeof(long)] = c => c.AsInt64().NotNullable(),
                [typeof(long?)] = c => c.AsInt64().Nullable(),
                [typeof(decimal)] = c => c.AsDecimal(18, 4).NotNullable(),
                [typeof(decimal?)] = c => c.AsDecimal(18, 4).Nullable(),
                [typeof(byte[])] = c => c.AsBinary(int.MaxValue).NotNullable(),
                [typeof(DateTime)] = c => c.AsDateTime2().NotNullable(),
                [typeof(DateTime?)] = c => c.AsDateTime2().Nullable(),
                [typeof(Guid)] = c => c.AsGuid().NotNullable(),
                [typeof(Guid?)] = c => c.AsGuid().Nullable()
            };
        }

        public virtual void CreateOrAlterTable<TEntity>(ICreateExpressionRoot createExpressionRoot, IAlterExpressionRoot alterExpressionRoot)
        {
            var type = typeof(TEntity);

            if (!TableExists(type.Name))
            {
                var createBuilder = createExpressionRoot.Table(type.Name) as CreateTableExpressionBuilder;

                if (createBuilder != null)
                {
                    CreateTableExpressions(type, createBuilder);
                }
            }
            else
            {
                var alterBuilder = alterExpressionRoot.Table(type.Name) as AlterTableExpressionBuilder;

                if (alterBuilder != null)
                {
                    _alterionTableManager.AlterTableExpressions(type, alterBuilder);
                }
            }
        }

        public virtual CreateTableExpression GetCreateTableExpression(Type type)
        {
            var expression = new CreateTableExpression { TableName = type.Name };
            var builder = new CreateTableExpressionBuilder(expression, CreateNullMigrationContext());

            CreateTableExpressions(type, builder);

            return builder.Expression;
        }

        /// <summary>
        /// Determine if table exists in schema
        /// </summary>
        /// <param name="tableName">name of the table</param>
        /// <returns>bool result</returns>
        protected bool TableExists(string tableName)
        {
            return _schemaExpressionRoot.Schema("dbo").Table(tableName).Exists();
        }


        /// <summary>
        /// Create database table
        /// </summary>
        /// <param name="type">entity type</param>
        /// <param name="builder">expression builder</param>
        protected void CreateTableExpressions(Type type, CreateTableExpressionBuilder builder)
        {
            if (!builder.Expression.Columns.Any(c => c.IsPrimaryKey))
            {
                var pk = new ColumnDefinition
                {
                    Name = nameof(TableEntity.Id),
                    Type = DbType.Int32,
                    IsIdentity = true,
                    TableName = type.Name,
                    ModificationType = ColumnModificationType.Create,
                    IsPrimaryKey = true
                };

                builder.Expression.Columns.Insert(0, pk);
                builder.CurrentColumn = pk;
            }

            var propertiesToMap = _appTypeFinder.GetEntityPropertiesToMap(type);

            foreach (var prop in propertiesToMap)
            {
                CreateTableColumnByEntityType(type, prop, builder);
            }
        }


        /// <summary>
        /// Create column by entity type
        /// </summary>
        /// <param name="type">entity type</param>
        /// <param name="propertyInfo">entity column property info</param>
        /// <param name="createBuilder">expression builder</param>
        protected virtual void CreateTableColumnByEntityType(
            Type type,
            PropertyInfo propertyInfo,
            CreateTableExpressionBuilder createBuilder)
        {
            if (!_createTypeMappings.ContainsKey(propertyInfo.PropertyType) && propertyInfo.PropertyType != typeof(string))
            {
                return;
            }

            var column = createBuilder.WithColumn(propertyInfo.Name);

            if (propertyInfo.PropertyType != typeof(string))
            {
                _createTypeMappings[propertyInfo.PropertyType](column);
            }

            ResolveCreateTableColumnAttributes(propertyInfo, createBuilder);
        }


        /// <summary>
        /// Configure expression builder by entity column attributes
        /// </summary>
        /// <param name="propertyInfo">entity column property info</param>
        /// <param name="createBuilder">expression builder</param>
        protected virtual void ResolveCreateTableColumnAttributes(
            PropertyInfo propertyInfo,
            CreateTableExpressionBuilder createBuilder)
        {
            object[] attributes = propertyInfo.GetCustomAttributes(true);

            if (propertyInfo.PropertyType == typeof(string))
            {
                var maxStringLengthAttribute = attributes.FirstOrDefault(x => x.GetType() == typeof(MaxStringLengthAttribute));
                var notNullableStringAttribute = attributes.FirstOrDefault(x => x.GetType() == typeof(NotNullableStringAttribute));

                if (maxStringLengthAttribute != null)
                {
                    var attributeStringData = maxStringLengthAttribute as MaxStringLengthAttribute;
                    createBuilder.AsString(attributeStringData.MaxStringLength);
                }
                else
                {
                    createBuilder.AsString(int.MaxValue);
                }

                if (notNullableStringAttribute != null)
                {
                    createBuilder.NotNullable();
                }
                else
                {
                    createBuilder.Nullable();
                }
            }

            foreach (var attribute in attributes)
            {
                var attributeType = attribute.GetType();

                switch (attributeType)
                {
                    case Type t when t == typeof(ForeignKeyAttribute):
                        if (propertyInfo.PropertyType == typeof(int) || propertyInfo.PropertyType == typeof(int?))
                        {
                            var attributeData = attribute as ForeignKeyAttribute;

                            if (attributeData != null)
                            {
                                createBuilder.ForeignKey(attributeData.TargetTableName, nameof(TableEntity.Id))
                                    .OnDeleteOrUpdate(Rule.None);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Create empty migration context
        /// </summary>
        /// <returns>migration context</returns>
        protected IMigrationContext CreateNullMigrationContext()
        {
            return new MigrationContext(new NullIfDatabaseProcessor(), _migrationContext.ServiceProvider, null, null);
        }
    }
}
