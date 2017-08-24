using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MultiIndexCollection
{
    internal static class ExpressionExtensions
    {
        /// <summary>
        /// Get computed value of Expression.
        /// </summary>
        /// <exception cref="NotSupportedException" />
        public static object GetValue(this Expression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            if (expression.NodeType == ExpressionType.Constant)
            {
                return ((ConstantExpression)expression).Value;
            }
            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var member = (MemberExpression)expression;

                if (member.Expression.NodeType == ExpressionType.Constant)
                {
                    var instance = (ConstantExpression)member.Expression;

                    if (instance.Type.GetTypeInfo().IsDefined(typeof(CompilerGeneratedAttribute)))
                    {
                        return instance.Type
                            .GetField(member.Member.Name)
                            .GetValue(instance.Value);
                    }
                }
            }

            // we can't interpret the expression but we can compile and run it
            var objectMember = Expression.Convert(expression, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            try
            {
                return getterLambda.Compile().Invoke();
            }
            catch (InvalidOperationException exception)
            {
                throw new NotSupportedException($"Value of {expression} can't be comuted", exception);
            }
        }

        /// <summary>
        /// Get member name from <see cref="MemberExpression"/>.
        /// </summary>
        /// <exception cref="NotSupportedException" />
        public static string GetMemberName(this Expression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            if (expression.NodeType != ExpressionType.MemberAccess)
            {
                throw new NotSupportedException($"Expression {expression} is not a Member Access");
            }

            return ((MemberExpression)expression).Member.Name;
        }
    }
}
