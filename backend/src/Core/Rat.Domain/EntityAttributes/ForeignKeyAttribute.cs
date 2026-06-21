using System;

namespace Rat.Domain.EntityAttributes
{
    /// <summary>
    /// Specify entity entry foreign key to another table (binding to primary ID key column)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public partial class ForeignKeyAttribute : Attribute
    {
        public string TargetTableName { get; set; }

        public ForeignKeyAttribute(string targetTableName)
        {
            TargetTableName = targetTableName;
        }
    }
}
