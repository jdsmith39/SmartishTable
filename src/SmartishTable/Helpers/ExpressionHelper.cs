using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace System.Linq.Expressions;

public static class ExpressionHelper
{
    internal static MemberExpression GetMemberExpression<SmartishTItem>(this Expression<Func<SmartishTItem, object>> expression)
    {
        if (!(expression.Body is MemberExpression body))
        {
            var ubody = (UnaryExpression)expression.Body;
            body = ubody.Operand as MemberExpression;
        }

        return body;
    }

    internal static string GetPropertyName<SmartishTItem>(this Expression<Func<SmartishTItem, object>> expression)
    {
        if (expression == null)
            return null;

        var body = GetMemberExpression(expression);

        var name = body?.Member?.Name;
        if (string.IsNullOrEmpty(name))
            throw new NotSupportedException($"{expression.Body.ToString()} is not a member expression. {expression.Body.NodeType}");

        return name;
    }

    internal static string GetPropertyName(this MemberExpression me, Type ignorePathUntil)
    {
        //var body = expression.Body;
        //while (body.NodeType == ExpressionType.Convert || body.NodeType == ExpressionType.ConvertChecked)
        //{
        //    body = ((UnaryExpression)body).Operand;
        //}

        var result = new List<string>();
        //var me = body as MemberExpression;

        if (me != null && me.Expression.NodeType == ExpressionType.MemberAccess)
        {
            var path = me.Expression as MemberExpression;
            Console.WriteLine($"me.Expression:  " + path.Member.Name);
            Console.WriteLine("Path.Expression.Type.FullName:  " + path.Expression.Type.FullName);
            var notFinalMember = ignorePathUntil != null && path.Expression.Type != ignorePathUntil;

            do
            {
                if (notFinalMember)
                {
                    result.Add(path.Member.Name);
                }
                //var propInfo = (path.Member as PropertyInfo);
                //Console.WriteLine("Prop type:  " + propInfo?.PropertyType.FullName);
                //if (notFinalMember && (path.Member as PropertyInfo)?.PropertyType == ignorePathUntil)
                //{
                //    notFinalMember = false;
                //}

                Console.WriteLine("Path.Expression " + path.Expression.Type.FullName);
                path = path.Expression as MemberExpression;
                Console.WriteLine("Path:  " + path?.Member.Name);

            } while (path != null && path.NodeType == ExpressionType.MemberAccess);
        }

        var propertyName = me?.Member.Name;
        result.Add(propertyName);

        if (result.Count == 0)
        {
            throw new InvalidOperationException("Expression does not refer to a property: " + me.ToString());
        }

        return string.Join(".", result);
    }

    internal static Expression CreatePropertyExpression<T>(string propertyPath)
    {
        var entityType = typeof(T);
        ParameterExpression paramExp = Expression.Parameter(entityType, "w");

        // The property name might be a nested expression like a.b.c
        var path = propertyPath.Split('.');
        MemberExpression lastMember = null;

        foreach (var p in path)
        {
            if (lastMember == null)
            {
                lastMember = Expression.Property(paramExp, p);
            }
            else
            {
                lastMember = Expression.Property(lastMember, p);
            }
        }

        return lastMember;
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
