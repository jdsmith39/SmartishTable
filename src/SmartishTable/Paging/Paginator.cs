using SmartishTable.Helpers;
using System;
using System.Collections.Generic;

namespace SmartishTable.Paging;

public class Paginator : BaseClass
{
    internal static HashSet<string> PaginatorPropertiesChangedList = new HashSet<string>() { nameof(Paginator.Page), nameof(Paginator.PageSize) };

    private int? pageSize;
    public int? PageSize 
    { 
        get => pageSize;
        set 
        {
            page = 1;
            SetProperty(ref pageSize, value); 
        }
    }

    internal int page;
    public int Page { get => page; set => SetProperty(ref page, value); }

    public int Count { get; internal set; }

    public int PageCount 
    {
        get
        {
            if (!PageSize.HasValue)
                return 1;
            return (int)Math.Ceiling((double)Count / (double)PageSize.Value);
        }
    }

    public bool IsPreviousPageEnabled { get => Page > 1; }

    public bool IsNextPageEnabled { get => Page < PageCount; }

    public int PageLowerBound 
    { 
        get
        {
            if (!PageSize.HasValue && Count == 0)
                return 0;
            else if (!PageSize.HasValue && Count > 0)
                return 1;

            return Count > 0 ? PageSize.Value * (Page - 1) + 1 : 0;
        }
    }

    public int PageUpperBound
    {
        get
        {
            if (!PageSize.HasValue)
                return Count;

            var upperBound = PageSize.Value * Page;
            return upperBound > Count ? Count : upperBound;
        }
    }
}
