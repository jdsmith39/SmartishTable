using System;
using System.Linq.Expressions;

namespace SmartishTable.Interfaces
{
    public interface IFilter<TItem>
    {
        Expression<Func<TItem, bool>> GetFilter();
    }
}
