using System;

namespace Rat.Domain.EntityAttributes
{
    /// <summary>
    /// Specify not nullable database string property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public partial class NotNullableStringAttribute : Attribute
    {
    
    }
}
