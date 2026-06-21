using System.Linq.Expressions;
using System.Threading.Tasks;
using LinqToDB;

namespace System.Linq
{
    public static class AsyncIQueryableExtensions
    {
        /// <summary>
        /// Returns the first element of a sequence, or a default value if the sequence contains no elements
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source</typeparam>
        /// <param name="source">Source</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the default(TSource) if source is empty; otherwise, the first element in source
        /// </returns>
        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source,
            Expression<Func<TSource, bool>> predicate = null)
        {
            return predicate == null ? AsyncExtensions.FirstOrDefaultAsync(source) : AsyncExtensions.FirstOrDefaultAsync(source, predicate);
        }
    }
}
