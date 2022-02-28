using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace System.Linq.Expressions;

public static class ExpressionHelper
{
    private static MemberExpression GetMemberExpression<SmartishTItem>(Expression<Func<SmartishTItem, object>> expression)
    {
        if (!(expression.Body is MemberExpression body))
        {
            var ubody = (UnaryExpression)expression.Body;
            body = ubody.Operand as MemberExpression;
        }

        return body;
    }

    internal static string GetPropertyName<SmartishTItem>(Expression<Func<SmartishTItem, object>> expression)
    {
        if (expression == null)
            return null;

        var body = GetMemberExpression(expression);

        var name = body?.Member?.Name;
        if (string.IsNullOrEmpty(name))
            throw new NotSupportedException($"{expression.Body.ToString()} is not a member expression. {expression.Body.NodeType}");

        return name;
    }

    internal static Type GetPropertyType<SmartishTItem>(Expression<Func<SmartishTItem, object>> expression)
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
