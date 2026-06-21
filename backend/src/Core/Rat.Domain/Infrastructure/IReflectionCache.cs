using Rat.Domain.Types;
using System;
using System.Reflection;

namespace Rat.Domain.Infrastructure
{
    /// <summary>
    /// Process-wide cache for structural reflection lookups (types, methods, properties).
    /// Registered as a singleton so the cached metadata is shared across all (scoped) consumers.
    /// </summary>
    public partial interface IReflectionCache
    {
        /// <summary>
        /// Get (or resolve and cache) the CLR type of an entity by its name
        /// </summary>
        /// <param name="entityName">entity name</param>
        /// <param name="valueFactory">factory resolving the type when not cached</param>
        /// <returns>the resolved entity type</returns>
        Type GetOrAddEntityType(string entityName, Func<string, Type> valueFactory);

        /// <summary>
        /// Get (or resolve and cache) a method by source type, method name and optional generic argument
        /// </summary>
        /// <param name="key">source type, method name and generic argument (null for non-generic)</param>
        /// <param name="valueFactory">factory resolving the method when not cached</param>
        /// <returns>the resolved method</returns>
        MethodInfo GetOrAddMethod((Type Source, string Method, Type Generic) key, Func<(Type Source, string Method, Type Generic), MethodInfo> valueFactory);

        /// <summary>
        /// Get (or read and cache) the public properties of a type
        /// </summary>
        /// <param name="type">the type to reflect</param>
        /// <param name="valueFactory">factory reading the properties when not cached</param>
        /// <returns>the type's public properties</returns>
        PropertyInfo[] GetOrAddProperties(Type type, Func<Type, PropertyInfo[]> valueFactory);

        /// <summary>
        /// Get (or resolve and cache) the metadata method for an entity name and class type
        /// </summary>
        /// <param name="key">entity name and class type</param>
        /// <param name="valueFactory">factory resolving the method (may return null) when not cached</param>
        /// <returns>the resolved metadata method, or null when none exists</returns>
        MethodInfo GetOrAddMetadataMethod((string EntityName, ClassType ClassType) key, Func<(string EntityName, ClassType ClassType), MethodInfo> valueFactory);
    }
}
