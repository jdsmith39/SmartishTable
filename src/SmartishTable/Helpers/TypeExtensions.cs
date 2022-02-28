﻿using System;

public static class TypeExtensions
{
    public static bool IsNumeric(this Type type)
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;
            case TypeCode.Object:
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return Nullable.GetUnderlyingType(type).IsNumeric();
                }
                return false;
            default:
                return false;
        }
    }

    public static Type GetNonNullableType(this Type type)
    {
        return Nullable.GetUnderlyingType(type) ?? type;
    }
}
