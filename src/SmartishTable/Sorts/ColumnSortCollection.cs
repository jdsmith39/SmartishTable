using System.Collections.Generic;
using System.Linq;

namespace SmartishTable.Sorts
{
    internal class ColumnSortCollection<SmartishTItem> : Dictionary<string, ColumnSortData<SmartishTItem>>
    {
        internal void Set(int maxNumberOfSorts, string field)
        {
            if (this[field].SortOrder.HasValue)
            {
                if (maxNumberOfSorts == 1)
                    this[field].Toggle();
                else
                {
                    if (!this[field].IsDescending)
                        this[field].Toggle();
                    else
                        this[field].Reset();
                }
            }
            else
            {
                var totalSortColumns = this.Values.Count(w => w.SortOrder.HasValue);
                if (totalSortColumns == maxNumberOfSorts)
                {
                    // at max, reset sort Order 1
                    var firstSort = Values.First(w => w.SortOrder == 1);
                    firstSort.Reset();
                }

                this[field].SortOrder = totalSortColumns + 1;
            }

            ReorderSortOrderNumbers();
        }

        internal void ReorderSortOrderNumbers()
        {
            var sortedColumns = Values.Where(w => w.SortOrder.HasValue).OrderBy(o => o.SortOrder.Value).ToList();
            for (int blah = 0; blah < sortedColumns.Count; blah++)
            {
                sortedColumns[blah].SortOrder = blah + 1;
            }
        }

        internal IQueryable<SmartishTItem> SetOrderBys(IQueryable<SmartishTItem> query)
        {
            var sortColumns = this.Values.Where(w => w.SortOrder.HasValue).OrderBy(o => o.SortOrder);
            IOrderedQueryable<SmartishTItem> orderedQuery = null;
            foreach (var item in sortColumns)
            {
                if (item.SortOrder == 1)
                {
                    if (item.IsDescending)
                    {
                        if (item.Comparer == null)
                            orderedQuery = query.OrderByDescending(item.Field);
                        else
                            orderedQuery = query.OrderByDescending(item.Field, item.Comparer);
                    }   
                    else
                    {
                        if (item.Comparer == null)
                            orderedQuery = query.OrderBy(item.Field);
                        else
                            orderedQuery = query.OrderBy(item.Field, item.Comparer);
                    }
                }
                else
                {
                    if (item.IsDescending)
                    {
                        orderedQuery = orderedQuery.ThenByDescending(item.Field);
                    }
                    else
                    {
                        orderedQuery = orderedQuery.ThenBy(item.Field);
                    }
                }
            }

            if (orderedQuery != null)
                return orderedQuery;

            // nothing sorted
            return query;
        }
    }
}
