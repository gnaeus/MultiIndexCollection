using System;
using System.Linq.Expressions;
using FastExpressionCompiler;

namespace MultiIndexCollection
{
    internal abstract class PropertyIndex<T, TProperty>
    {
        public string MemberName { get; }

        readonly Func<T, TProperty> _getKey;

        /// <exception cref="NotSupportedException" />
        protected PropertyIndex(Expression<Func<T, TProperty>> lambdaExpression)
        {
            var memberExpression = lambdaExpression.Body as MemberExpression;

            if (memberExpression == null || memberExpression.NodeType != ExpressionType.MemberAccess)
            {
                throw new NotSupportedException($"Expression {lambdaExpression} is not a Member Access");
            }

            MemberName = memberExpression.Member.Name;

            _getKey = lambdaExpression.CompileFast();
        }

        public object GetKey(T item)
        {
            return _getKey.Invoke(item);
        }
    }
}
