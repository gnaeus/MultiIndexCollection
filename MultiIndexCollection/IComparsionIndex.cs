using System.Collections.Generic;
using System.Linq.Expressions;

namespace MultiIndexCollection
{
    internal interface IComparsionIndex<T> : IEqualityIndex<T>
    {
        IEnumerable<T> Filter(object key, ExpressionType type);
    }
}
