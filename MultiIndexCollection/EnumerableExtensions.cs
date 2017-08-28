using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MultiIndexCollection
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Make <see cref="IndexedCollection{T}"/> from <paramref name="enumerable"/>. It provides
        /// fast indexed search by using <see cref="IndexedCollection{T}.Where(Expression{Func{T, bool}})"/>
        /// </summary>
        /// <exception cref="NotSupportedException" />
        public static IndexedCollection<TSource> IndexBy<TSource, TProperty>(
            this IEnumerable<TSource> enumerable,
            Expression<Func<TSource, TProperty>> property,
            bool isSorted = false)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
            if (property == null) throw new ArgumentNullException(nameof(property));

            var collection = enumerable as IndexedCollection<TSource>
                ?? new IndexedCollection<TSource>(enumerable);

            return collection.IndexBy(property, isSorted);
        }
    }
}
