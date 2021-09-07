using SmartishTable.Helpers;

namespace SmartishTable.Filters
{
    public class FilterContext<T> : BaseClass
    {
        private T _filterValue;
        public T FilterValue
        {
            get { return _filterValue; }
            set { SetProperty(ref _filterValue, value); } 
        }
    }
}
