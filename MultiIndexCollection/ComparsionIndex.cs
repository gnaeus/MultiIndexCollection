using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MultiIndexCollection
{
    internal class ComparsionIndex<T, TProperty> : PropertyIndex<T, TProperty>, IComparsionIndex<T>
    {
        // TODO: implement
        // TODO: store items with `null` key

        /// <exception cref="NotSupportedException" />
        public ComparsionIndex(Expression<Func<T, TProperty>> lambda)
            : base(lambda)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Filter(object key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Filter(object key, ExpressionType type)
        {
            throw new NotImplementedException();
        }

        public void Add(object key, T item)
        {
            throw new NotImplementedException();
        }

        public void Remove(object key, T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }
}
