using System;
using FluentMigrator.Builders.Alter.Table;

namespace Rat.DataStorage.Migrations
{
    public partial interface IAlterationTableManager
    {
        /// <summary>
        /// Add missing columns to table
        /// </summary>
        /// <param name="type">entity type</param>
        /// <param name="builder">expression builder</param>
        void AlterTableExpressions(Type type, AlterTableExpressionBuilder builder);
    }
}
