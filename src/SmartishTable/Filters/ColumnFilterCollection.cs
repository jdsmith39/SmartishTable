﻿using SmartishTable.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartishTable.Filters
{
    internal class ColumnFilterCollection<TItem> : List<IFilter<TItem>>
    {
        internal IQueryable<TItem> SetFilters(IQueryable<TItem> query)
        {
            foreach (var item in this)
            {
                var filter = item.GetFilter();
                if (filter != null)
                    query = query.Where(filter);
            }

            return query;
        }
    }
}
