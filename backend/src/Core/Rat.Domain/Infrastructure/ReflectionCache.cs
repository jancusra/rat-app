using Rat.Domain.Types;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Rat.Domain.Infrastructure
{
    /// <summary>
    /// Process-wide cache for structural reflection lookups (types, methods, properties).
    /// Registered as a singleton so the cached metadata is shared across all (scoped) consumers.
    /// </summary>
    public partial class ReflectionCache : IReflectionCache
    {
        private readonly ConcurrentDictionary<string, Type> _entityTypeCache = new ConcurrentDictionary<string, Type>();

        private readonly ConcurrentDictionary<(Type Source, string Method, Type Generic), MethodInfo> _methodCache =
            new ConcurrentDictionary<(Type, string, Type), MethodInfo>();

        private readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new ConcurrentDictionary<Type, PropertyInfo[]>();

        private readonly ConcurrentDictionary<(string EntityName, ClassType ClassType), MethodInfo> _metadataMethodCache =
            new ConcurrentDictionary<(string, ClassType), MethodInfo>();

        public Type GetOrAddEntityType(string entityName, Func<string, Type> valueFactory)
        {
            return _entityTypeCache.GetOrAdd(entityName, valueFactory);
        }

        public MethodInfo GetOrAddMethod(
            (Type Source, string Method, Type Generic) key,
            Func<(Type Source, string Method, Type Generic), MethodInfo> valueFactory)
        {
            return _methodCache.GetOrAdd(key, valueFactory);
        }

        public PropertyInfo[] GetOrAddProperties(Type type, Func<Type, PropertyInfo[]> valueFactory)
        {
            return _propertyCache.GetOrAdd(type, valueFactory);
        }

        public MethodInfo GetOrAddMetadataMethod(
            (string EntityName, ClassType ClassType) key,
            Func<(string EntityName, ClassType ClassType), MethodInfo> valueFactory)
        {
            return _metadataMethodCache.GetOrAdd(key, valueFactory);
        }
    }
}
