using Rat.Domain.Types;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rat.Domain.Infrastructure
{
    /// <summary>
    /// Class definition for the reflection operation (usually used for dynamic library scanning)
    /// </summary>
    public partial interface IAppTypeFinder
    {
        /// <summary>
        /// Find a specific classes in all project libraries
        /// </summary>
        /// <typeparam name="T">class to find</typeparam>
        /// <param name="onlyConcreteClasses">only concrete classes (not abstract)</param>
        /// <returns>found types of specific class</returns>
        IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true);

        /// <summary>
        /// Get full assembly name by specific class name (usually an entity name)
        /// </summary>
        /// <param name="className">class/entity name</param>
        /// <param name="classType">the type of class to get</param>
        /// <returns>full assembly name</returns>
        string GetAssemblyQualifiedNameByClass(string className, ClassType classType = ClassType.Class);

        /// <summary>
        /// Getting entity properties for mapping as table columns
        /// </summary>
        /// <param name="type">entity type</param>
        /// <returns>properties to map as columns</returns>
        PropertyInfo[] GetEntityPropertiesToMap(Type type);
    }
}
