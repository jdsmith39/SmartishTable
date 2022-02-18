using SmartishTable.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace SmartishTable.Filters
{
    internal class ColumnFilterCollection<SmartishTItem> : List<IFilter<SmartishTItem>>
    {
        internal IQueryable<SmartishTItem> SetFilters(IQueryable<SmartishTItem> query)
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
