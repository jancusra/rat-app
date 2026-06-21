using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using FluentMigrator.Builders.Alter.Table;
using FluentMigrator.Builders.Schema;
using Rat.Domain;
using Rat.Domain.EntityAttributes;
using Rat.Domain.Infrastructure;

namespace Rat.DataStorage.Migrations
{
    /// <summary>
    /// Class to define table migration alternation (usually adding missing columns)
    /// </summary>
    public partial class AlterationTableManager : IAlterationTableManager
    {
        private readonly IAppTypeFinder _appTypeFinder;
        private readonly ISchemaExpressionRoot _schemaExpressionRoot;

        private readonly Dictionary<Type, Action<IAlterTableColumnAsTypeSyntax>> _alterTypeMappings;

        public AlterationTableManager(
            IAppTypeFinder appTypeFinder,
            ISchemaExpressionRoot schemaExpressionRoot)
        {
            _appTypeFinder = appTypeFinder;
            _schemaExpressionRoot = schemaExpressionRoot;

            _alterTypeMappings = new Dictionary<Type, Action<IAlterTableColumnAsTypeSyntax>>
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

        public virtual void AlterTableExpressions(Type type, AlterTableExpressionBuilder builder)
        {
            var propertiesToMap = _appTypeFinder.GetEntityPropertiesToMap(type);

            foreach (var prop in propertiesToMap)
            {
                if (!ColumnExists(type.Name, prop.Name))
                {
                    AddColumnByEntityType(type, prop, builder);
                }
            }
        }

        /// <summary>
        /// Determine if table column already exists
        /// </summary>
        /// <param name="tableName">name of the table</param>
        /// <param name="columnName">name of the column</param>
        /// <returns></returns>
        protected bool ColumnExists(string tableName, string columnName)
        {
            return _schemaExpressionRoot.Schema("dbo").Table(tableName).Column(columnName).Exists();
        }

        /// <summary>
        /// Add column by entity type
        /// </summary>
        /// <param name="type">entity type</param>
        /// <param name="propertyInfo">entity column property info</param>
        /// <param name="alterBuilder">expression builder</param>
        protected virtual void AddColumnByEntityType(
            Type type,
            PropertyInfo propertyInfo,
            AlterTableExpressionBuilder alterBuilder)
        {
            if (!_alterTypeMappings.ContainsKey(propertyInfo.PropertyType) && propertyInfo.PropertyType != typeof(string))
            {
                return;
            }

            var column = alterBuilder.AddColumn(propertyInfo.Name);

            if (propertyInfo.PropertyType != typeof(string))
            {
                _alterTypeMappings[propertyInfo.PropertyType](column);
            }

            ResolveAlterTableColumnAttributes(propertyInfo, alterBuilder);
        }

        /// <summary>
        /// Configure expression builder by entity column attributes
        /// </summary>
        /// <param name="propertyInfo">entity column property info</param>
        /// <param name="alterBuilder">expression builder</param>
        protected virtual void ResolveAlterTableColumnAttributes(
            PropertyInfo propertyInfo,
            AlterTableExpressionBuilder alterBuilder)
        {
            object[] attributes = propertyInfo.GetCustomAttributes(true);

            if (propertyInfo.PropertyType == typeof(string))
            {
                var maxStringLengthAttribute = attributes.FirstOrDefault(x => x.GetType() == typeof(MaxStringLengthAttribute));
                var notNullableStringAttribute = attributes.FirstOrDefault(x => x.GetType() == typeof(NotNullableStringAttribute));

                if (maxStringLengthAttribute != null)
                {
                    var attributeStringData = maxStringLengthAttribute as MaxStringLengthAttribute;
                    alterBuilder.AsString(attributeStringData.MaxStringLength);
                }
                else
                {
                    alterBuilder.AsString(int.MaxValue);
                }

                if (notNullableStringAttribute != null)
                {
                    alterBuilder.NotNullable();
                }
                else
                {
                    alterBuilder.Nullable();
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
                                alterBuilder.ForeignKey(attributeData.TargetTableName, nameof(TableEntity.Id))
                                    .OnDeleteOrUpdate(Rule.None);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
