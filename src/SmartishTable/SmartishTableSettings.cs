using SmartishTable.Sorts;
using System.Collections.Generic;

namespace SmartishTable;

public class SmartishTableSettings
{
    public List<ColumnSort>? ColumnSorts { get; set; }

    public int? PageSize { get; set; }

    public int? Page { get; set; }
}
