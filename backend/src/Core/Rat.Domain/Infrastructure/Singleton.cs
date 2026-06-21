using System;

namespace Rat.Domain.Infrastructure
{
    /// <summary>
    /// A statically compiled "singleton" used to store objects throughout the
    /// lifetime of the app domain. Not so much singleton in the pattern's
    /// sense of the word as a standardized way to store single instances.
    /// </summary>
    /// <typeparam name="T">The type of object to store.</typeparam>
    /// <remarks>
    /// Direct access through <see cref="Instance"/> is not synchronized. To create the
    /// instance lazily from multiple threads without racing, use <see cref="GetOrCreate"/>.
    /// </remarks>
    public sealed class Singleton<T> where T : class
    {
        private static readonly object _lock = new object();
        private static volatile T instance;

        /// <summary>
        /// The singleton instance for the specified type T. Only one instance (at the time) of this object for each type of T.
        /// </summary>
        public static T Instance
        {
            get
            {
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        /// <summary>
        /// Returns the existing instance, or atomically creates and stores one using
        /// <paramref name="factory"/>. The factory runs at most once even when several
        /// threads call concurrently (double-checked locking).
        /// </summary>
        /// <param name="factory">Factory invoked to build the instance when none exists yet.</param>
        /// <returns>The shared instance for type T.</returns>
        public static T GetOrCreate(Func<T> factory)
        {
            if (instance is not null)
            {
                return instance;
            }

            lock (_lock)
            {
                instance ??= factory();
            }

            return instance;
        }
    }
}
