using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SmartishTable.Sorts;

internal class ColumnSortData<SmartishTItem> : ColumnSort
{
    [JsonIgnore]
    public System.Linq.Expressions.Expression<Func<SmartishTItem, object>> Field { get; set; } = default!;

    [JsonIgnore]
    public IComparer<object>? Comparer { get; set; }

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
