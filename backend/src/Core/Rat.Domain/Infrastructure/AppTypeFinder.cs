using System;
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

        public IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(typeof(T), onlyConcreteClasses);
        }

        public virtual string GetAssemblyQualifiedNameByClass(string className, ClassType classType = ClassType.Class)
        {
            var assemblies = GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                var assemblyTypes = assembly.GetTypes();

                switch (classType)
                {
                    case ClassType.Class:
                        {
                            if (assemblyTypes.FirstOrDefault(x => x.Name == className) != null)
                            {
                                return assemblyTypes.FirstOrDefault(x => x.Name == className).AssemblyQualifiedName;
                            }

                            break;
                        };
                    default:
                        {
                            if (assemblyTypes.FirstOrDefault(x => x.FullName.EndsWith($"{classType}.{className}")) != null)
                            {
                                return assemblyTypes.FirstOrDefault(x => x.FullName.EndsWith($"{classType}.{className}")).AssemblyQualifiedName;
                            }

                            break;
                        };
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