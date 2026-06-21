using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Rat.Domain.Types;

namespace Rat.Domain.Infrastructure
{
    /// <summary>
    /// Class definition for the reflection operation (usually used for dynamic library scanning)
    /// </summary>
    public partial class AppTypeFinder : IAppTypeFinder
    {
        /// <summary>
        /// Prefix for libraries scanned within the Rat project
        /// </summary>
        private string RatAssembliesShouldStartsWith { get; set; } = "Rat.";

        /// <summary>
        /// Cache of resolved assembly-qualified names keyed by "{classType}:{className}".
        /// Avoids re-scanning every assembly's types on each lookup. Static so it is shared
        /// across instances; only successful resolutions are cached (so a not-yet-loaded
        /// assembly can still be found on a later call).
        /// </summary>
        private static readonly ConcurrentDictionary<string, string> _assemblyQualifiedNameCache = new();

        /// <summary>
        /// Cached set of resolved Rat assemblies. Built once (disk scan + load) and shared
        /// across instances so repeated <see cref="FindClassesOfType{T}"/> calls at startup
        /// don't re-scan the base directory.
        /// </summary>
        private static IList<Assembly> _allAssembliesCache;

        private static readonly object _allAssembliesLock = new object();

        public IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(typeof(T), onlyConcreteClasses);
        }

        public virtual string GetAssemblyQualifiedNameByClass(string className, ClassType classType = ClassType.Class)
        {
            var cacheKey = $"{classType}:{className}";

            if (_assemblyQualifiedNameCache.TryGetValue(cacheKey, out var cachedName))
            {
                return cachedName;
            }

            var resolvedName = ResolveAssemblyQualifiedNameByClass(className, classType);

            if (!string.IsNullOrEmpty(resolvedName))
            {
                _assemblyQualifiedNameCache[cacheKey] = resolvedName;
            }

            return resolvedName;
        }

        /// <summary>
        /// Scan all Rat assemblies for a matching type and return its assembly-qualified name.
        /// </summary>
        /// <param name="className">name of the class to find</param>
        /// <param name="classType">kind of class, used to build the match</param>
        /// <returns>assembly-qualified name, or empty string when not found</returns>
        private string ResolveAssemblyQualifiedNameByClass(string className, ClassType classType)
        {
            foreach (var assembly in GetAssemblies())
            {
                var match = classType == ClassType.Class
                    ? assembly.GetTypes().FirstOrDefault(x => x.Name == className)
                    : assembly.GetTypes().FirstOrDefault(x => x.FullName != null && x.FullName.EndsWith($"{classType}.{className}"));

                if (match != null)
                {
                    return match.AssemblyQualifiedName;
                }
            }

            return string.Empty;
        }

        public virtual PropertyInfo[] GetEntityPropertiesToMap(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.SetProperty);
        }

        /// <summary>
        /// Find a specific classes in all project libraries
        /// </summary>
        /// <param name="assignedType">the type of class to find</param>
        /// <param name="onlyConcreteClasses">only concrete classes (not abstract)</param>
        /// <returns>found types of specific class</returns>
        /// <exception cref="Exception"></exception>
        protected virtual IEnumerable<Type> FindClassesOfType(Type assignedType, bool onlyConcreteClasses = true)
        {
            var assemblies = GetAllAssemblies();
            var classes = new List<Type>();

            try
            {
                foreach (var assembly in assemblies)
                {
                    Type[] types = null;

                    try
                    {
                        types = assembly.GetTypes();
                    }
                    catch { }

                    if (types != null)
                    {
                        foreach (var type in types)
                        {
                            if (!assignedType.IsAssignableFrom(type)
                                && (!assignedType.IsGenericTypeDefinition || !DoesTypeImplementOpenGeneric(type, assignedType)))
                                continue;

                            if (type.IsInterface)
                                continue;

                            if (onlyConcreteClasses)
                            {
                                if (type.IsClass && !type.IsAbstract)
                                {
                                    classes.Add(type);
                                }
                            }
                            else
                            {
                                classes.Add(type);
                            }
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                var message = string.Empty;

                foreach (var e in ex.LoaderExceptions)
                {
                    message += e.Message + Environment.NewLine;
                }

                throw new Exception(message, ex);
            }

            return classes;
        }

        /// <summary>
        /// Get all Rat project assemblies
        /// </summary>
        /// <returns>list of all Rat project assemblies</returns>
        protected virtual IList<Assembly> GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.FullName.StartsWith(RatAssembliesShouldStartsWith)).ToList();
        }

        /// <summary>
        /// Get all application assemblies
        /// </summary>
        /// <returns>list of all application assemblies</returns>
        protected virtual IList<Assembly> GetAllAssemblies()
        {
            if (_allAssembliesCache != null)
            {
                return _allAssembliesCache;
            }

            lock (_allAssembliesLock)
            {
                if (_allAssembliesCache != null)
                {
                    return _allAssembliesCache;
                }

                _allAssembliesCache = LoadAllAssemblies();
                return _allAssembliesCache;
            }
        }

        /// <summary>
        /// Scan the base directory, load any not-yet-loaded DLLs and return all Rat assemblies.
        /// </summary>
        /// <returns>list of all Rat project assemblies</returns>
        private IList<Assembly> LoadAllAssemblies()
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var loadedPaths = loadedAssemblies.Select(a => a.Location).ToArray();

            var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            var toLoadAssemblies = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();
            var resultAssemblies = new List<Assembly>();

            toLoadAssemblies.ForEach(path => loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path))));

            foreach (var assembly in loadedAssemblies)
            {
                if (assembly.FullName.StartsWith(RatAssembliesShouldStartsWith)
                    && resultAssemblies.FirstOrDefault(x => x.FullName == assembly.FullName) == null)
                {
                    resultAssemblies.Add(assembly);
                }
            }

            return resultAssemblies;
        }

        /// <summary>
        /// Determine whether the type implements an open generic
        /// </summary>
        /// <param name="type">input type</param>
        /// <param name="openGeneric">open generic type</param>
        /// <returns>the bool result</returns>
        protected virtual bool DoesTypeImplementOpenGeneric(Type type, Type openGeneric)
        {
            try
            {
                var genericTypeDefinition = openGeneric.GetGenericTypeDefinition();
                foreach (var implementedInterface in type.FindInterfaces((objType, objCriteria) => true, null))
                {
                    if (!implementedInterface.IsGenericType)
                        continue;

                    if (genericTypeDefinition.IsAssignableFrom(implementedInterface.GetGenericTypeDefinition()))
                        return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}