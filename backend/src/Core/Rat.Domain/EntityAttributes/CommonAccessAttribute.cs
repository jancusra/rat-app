using System;

namespace Rat.Domain.EntityAttributes
{
    /// <summary>
    /// Specify whether the entity has allowed common access
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public partial class CommonAccessAttribute : Attribute
    {

    }
}
