using System.ComponentModel.DataAnnotations;

namespace SmartishTable.Sorts;

public class ColumnSort
{
    /// <summary>
    /// Column name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Is Descending if true
    /// </summary>
    public bool IsDescending { get; set; }

    /// <summary>
    /// Null = no sort.
    /// > 0 equals sort order
    /// If same number, it goes in execution order
    /// </summary>
    [Range(1, int.MaxValue)]
    public int? SortOrder { get; set; }
}
