using System;
using FluentMigrator.Builders.Alter;
using FluentMigrator.Builders.Create;
using FluentMigrator.Expressions;

namespace Rat.DataStorage.Migrations
{
    public partial interface ICreationTableManager
    {
        /// <summary>
        /// Create or modify table (migration process)
        /// </summary>
        /// <typeparam name="TEntity">entity type</typeparam>
        /// <param name="createExpressionRoot">the root expression for a CREATE operation</param>
        /// <param name="alterExpressionRoot">the root expression interface for the alterations</param>
        void CreateOrAlterTable<TEntity>(ICreateExpressionRoot createExpressionRoot, IAlterExpressionRoot alterExpressionRoot);

        /// <summary>
        /// Get create table expression
        /// </summary>
        /// <param name="type">entity type</param>
        /// <returns>create table expression</returns>
        CreateTableExpression GetCreateTableExpression(Type type);
    }
}
