using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SmartishTable.Interfaces
{
    public interface IFilter<TItem>
    {
        Expression<Func<TItem, bool>> GetFilter();
    }
}
