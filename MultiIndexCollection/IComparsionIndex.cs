using System.Collections.Generic;

namespace MultiIndexCollection
{
    internal interface IComparsionIndex<T> : IEqualityIndex<T>, IEnumerable<T>
    {
        IEnumerable<T> GreaterThan(object key, bool exclusive);

        IEnumerable<T> LessThan(object key, bool exclusive);

        IEnumerable<T> Between(object keyFrom, bool excludeFrom, object keyTo, bool excludeTo);
        
        IEnumerable<T> HavingMin();

        IEnumerable<T> HavingMax();

        IEnumerable<T> Reverse();

        /// <exception cref="InvalidOperationException" />
        object Min();

        /// <exception cref="InvalidOperationException" />
        object Max();
    }
}
