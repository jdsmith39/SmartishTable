using System.Collections.Generic;
using System.Reflection;

namespace System.Linq.Expressions;

public static class ExpressionHelper
{
    internal static MemberExpression? GetMemberExpression<SmartishTItem>(this Expression<Func<SmartishTItem, object>> expression)
    {
        if (!(expression.Body is MemberExpression body))
        {
            var ubody = (UnaryExpression)expression.Body;
            body = ubody.Operand as MemberExpression;
        }

        return body;
    }

    internal static string GetPropertyName<SmartishTItem>(this Expression<Func<SmartishTItem, object>> expression, Type? ignorePathUntil)
    {
        var result = new List<string?>();
        var memberExpression = GetMemberExpression(expression);
        if (memberExpression != null && memberExpression.Expression!.NodeType == ExpressionType.MemberAccess)
        {
            var path = memberExpression.Expression as MemberExpression;
            var notFinalMember = ignorePathUntil != null && path!.Expression!.Type != ignorePathUntil;

            do
            {
                if (notFinalMember)
                {
                    result.Add(path!.Member.Name);
                }

                path = path!.Expression as MemberExpression;

            } while (path != null && path.NodeType == ExpressionType.MemberAccess);
        }

        var propertyName = memberExpression?.Member.Name;
        result.Add(propertyName);

        if (result.Count == 0)
        {
            throw new InvalidOperationException("Expression does not refer to a property: " + memberExpression?.ToString());
        }

        return string.Join(".", result);
    }

    /// <summary>
    /// Takes the path and goes down it to get to the last member expression.
    /// </summary>
    /// <param name="propertyPath"><see cref="string"/> propertyPath i.e.  x.Obj.Property</param>
    /// <param name="paramExp">parameter expression</param>
    /// <returns><see cref="MemberExpression"/></returns>
    internal static Expression GetLastMemberExpression(string propertyPath, ParameterExpression paramExp)
    {
        Expression lastMember = paramExp;

        foreach (var p in propertyPath.Split('.'))
        {
            lastMember = Expression.Property(lastMember, p);
        }

        return lastMember;
    }

    internal static Type? GetPropertyType<SmartishTItem>(Expression<Func<SmartishTItem, object>> expression)
    {
        if (expression == null)
            return null;

        var body = GetMemberExpression(expression);

        var propertyInfo = body?.Member as PropertyInfo;
        if (propertyInfo == null)
            throw new NotSupportedException($"{expression.Body.ToString()} is not a member expression. {expression.Body.NodeType}");

        return propertyInfo.PropertyType;
    }

    internal static BinaryExpression CreateNullChecks(this Expression expression, bool skipFinalMember = false)
    {
        var parents = new Stack<BinaryExpression>();

        BinaryExpression newExpression = null;

        if (expression is UnaryExpression unary)
        {
            expression = unary.Operand;
        }

        MemberExpression temp = expression as MemberExpression;

        while (temp is MemberExpression member)
        {
            try
            {
                var nullCheck = Expression.NotEqual(temp, Expression.Constant(null));
                parents.Push(nullCheck);
            }
            catch (InvalidOperationException) { }

            temp = member.Expression as MemberExpression;
        }

        while (parents.Count > 0)
        {
            if (skipFinalMember && parents.Count == 1 && newExpression != null)
                break;
            else if (newExpression == null)
                newExpression = parents.Pop();
            else
                newExpression = Expression.AndAlso(newExpression, parents.Pop());
        }

        if (newExpression == null)
        {
            return Expression.Equal(Expression.Constant(true), Expression.Constant(true));
        }

        return newExpression;
    }
}
