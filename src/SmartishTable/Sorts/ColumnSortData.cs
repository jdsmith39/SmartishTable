using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartishTable.Sorts;

internal class ColumnSortData<SmartishTItem>
{
    public System.Linq.Expressions.Expression<Func<SmartishTItem, object>> Field { get; set; }
    
    public bool IsDescending { get; set; }

    /// <summary>
    /// Null = no sort.
    /// > 0 equals sort order
    /// If same number, it goes in execution order
    /// </summary>
    [Range(1, int.MaxValue)]
    public int? SortOrder { get; set; }

    public IComparer<object> Comparer { get; set; }

    internal void Reset()
    {
        IsDescending = false;
        SortOrder = null;
    }

    internal void Toggle()
    {
        IsDescending = !IsDescending;
    }
}
