using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FastExpressionCompiler;

namespace MultiIndexCollection
{
    internal class ComparsionIndex<T, TProperty> : IComparsionIndex<T>
    {
        // TODO: implement
        // TODO: store items with `null` key

        public string MemberName { get; }

        readonly Func<T, TProperty> _getKey;

        /// <exception cref="NotSupportedException" />
        public ComparsionIndex(Expression<Func<T, TProperty>> lambda)
        {
            var memberExpression = lambda.Body as MemberExpression;

            if (memberExpression == null || memberExpression.NodeType != ExpressionType.MemberAccess)
            {
                throw new NotSupportedException($"Expression {lambda} is not a Member Access");
            }

            MemberName = memberExpression.Member.Name;

            _getKey = lambda.CompileFast();
        }

        public object GetKey(T item)
        {
            return _getKey.Invoke(item);
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
