using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SmartishTable.Sorts;

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

    private IOrderedEnumerable<ColumnSortData<SmartishTItem>> GetSortColumns()
    {
        return this.Values.Where(w => w.SortOrder.HasValue).OrderBy(o => o.SortOrder);
    }

    internal void RemoveHighestSortOrders(int maxNumberOfSorts)
    {
        var sortColumns = GetSortColumns();
        if (sortColumns.Count() <= maxNumberOfSorts)
            return;

        foreach (var item in sortColumns.OrderByDescending(o => o.SortOrder))
        {
            if (maxNumberOfSorts < item.SortOrder!.Value)
                item.SortOrder = null;
            else
                break;
        }
    }

    internal IQueryable<SmartishTItem> SetOrderBys(IQueryable<SmartishTItem> query)
    {
        var sortColumns = GetSortColumns();
        IOrderedQueryable<SmartishTItem>? orderedQuery = null;
        foreach (var item in sortColumns)
        {
            Expression<Func<SmartishTItem, object>>? lambda = null;
            // assumption is if the comparer exists, it is handling all edge cases.
            if (item.Comparer is null)
            {
                var fieldType = ExpressionHelper.GetPropertyType(item.Field)?.GetNonNullableType();
                if (fieldType is null)
                    throw new Exception($"{item.Field} type is null for sorting");
                var propertyPath = item.Field.GetPropertyName(fieldType);
                var paramExp = Expression.Parameter(typeof(SmartishTItem), "o");
                var filterProperty = ExpressionHelper.GetLastMemberExpression(propertyPath, paramExp);
                var filterPropertyConverted = Expression.Convert(filterProperty, typeof(object));
                lambda = Expression.Lambda<Func<SmartishTItem, object>>(Expression.Condition(filterProperty.CreateNullChecks(), filterPropertyConverted, Expression.Constant(null)), paramExp);
            }
            else
                lambda = item.Field;

            if (item.SortOrder == 1)
            {
                if (item.IsDescending)
                    orderedQuery = query.OrderByDescending(lambda, item.Comparer);
                else
                    orderedQuery = query.OrderBy(lambda, item.Comparer);
            }
            else
            {
                if (item.IsDescending)
                    orderedQuery = orderedQuery!.ThenByDescending(lambda, item.Comparer);
                else
                    orderedQuery = orderedQuery!.ThenBy(lambda, item.Comparer);
            }
        }

        if (orderedQuery != null)
            return orderedQuery;

        // nothing sorted
        return query;
    }

    internal List<ColumnSort> GetSortSettings()
    {
        return GetSortColumns().Select(s =>
        {
            if (string.IsNullOrEmpty(s.Name))
            {
                var type = ExpressionHelper.GetPropertyType(s.Field);
                s.Name = ExpressionHelper.GetPropertyName(s.Field, type);
            }

            var x = new ColumnSort()
            {
                IsDescending = s.IsDescending,
                SortOrder = s.SortOrder,
                Name = s.Name
            };

            return x;
        }).ToList();
    }

    internal void SetSortSettings(int maxNumberOfSorts, List<ColumnSort>? columns)
    {
        // clear existing sorts
        foreach (var item in Values)
        {
            if (string.IsNullOrEmpty(item.Name))
            {
                var type = ExpressionHelper.GetPropertyType(item.Field);
                item.Name = ExpressionHelper.GetPropertyName(item.Field, type);
            }
            item.IsDescending = false;
            item.SortOrder = null;
        }

        // nothing to set
        if (columns == null)
            return;

        // apply new sorts, can't go above max number sorts allowed
        var counter = 0;
        foreach (var item in from v in Values
                             join c in columns on v.Name equals c.Name
                             orderby c.SortOrder
                             select new { v, c })
        {
            item.v.IsDescending = item.c.IsDescending;
            item.v.SortOrder = item.c.SortOrder;

            counter++;
            if (maxNumberOfSorts == counter)
                break;
        }
    }
}
