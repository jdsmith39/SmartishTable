using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

public static class EnumExtensions
{
    /// <summary>
    /// returns the Enum value name, if it has a display attribute it returns that instead.
    /// </summary>
    /// <param name="en">Enum</param>
    /// <returns>string name</returns>
    public static string GetDisplayName(this Enum en)
    {
        var display = en.GetType().GetMember(en.ToString()).First().GetCustomAttributes(false).OfType<DisplayAttribute>().LastOrDefault();
        return display?.GetName() ?? en.ToString();
    }

    /// <summary>
    /// returns the Enum value description, if it has a description attribute it returns that instead.
    /// </summary>
    /// <param name="en">Enum</param>
    /// <returns>string description</returns>
    public static string GetDescription(this Enum en)
    {
        var description = en.GetType().GetMember(en.ToString()).First().GetCustomAttributes(false).OfType<DescriptionAttribute>().LastOrDefault();
        return description?.Description ?? en.ToString();
    }

    /// <summary>
    /// gets all possible enum values
    /// </summary>
    /// <typeparam name="T">enum type</typeparam>
    /// <param name="enumType">enum value</param>
    /// <returns>List of T (Enum) else null</returns>
    public static List<T> GetList<T>(this T enumType) where T : struct, IConvertible
    {
        if (typeof(T).IsEnum)
            return Enum.GetValues(enumType.GetType()).Cast<T>().ToList();

        return null;
    }
}
