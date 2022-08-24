using System;
using System.Linq.Expressions;

namespace SmartishTable.Interfaces;

public interface IFilter<SmartishTItem>
{
    Expression<Func<SmartishTItem, bool>> GetFilter();
}
