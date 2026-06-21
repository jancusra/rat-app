using System;

namespace Rat.Domain.EntityAttributes
{
    /// <summary>
    /// Maximum possible length of the database string
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public partial class MaxStringLengthAttribute : Attribute
    {
        public int MaxStringLength { get; set; }

        public MaxStringLengthAttribute(int maxStringLength = EntityDefaults.MaxTypicalStringLength)
        {
            MaxStringLength = maxStringLength;
        }
    }
}
