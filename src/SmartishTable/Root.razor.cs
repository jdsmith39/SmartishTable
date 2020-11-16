using Microsoft.AspNetCore.Components;
using SmartishTable.Filters;
using SmartishTable.Interfaces;
using SmartishTable.Paging;
using SmartishTable.Sorts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartishTable
{
    public partial class Root<TItem> : IDisposable
    {
        [Parameter]
        public List<TItem> SafeList { get; set; }

        internal List<TItem>? DisplayList { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public string SortAscendingCss { get; set; } = "smartish-table-sort-asc";

        [Parameter]
        public string SortDescendingCss { get; set; } = "smartish-table-sort-desc";

        [Parameter]
        public string NoSortCss { get; set; } = "";

        /// <summary>
        /// true if you want to use the SortAscendingCss, SortDescendingCss and NoSortCss values
        /// </summary>
        [Parameter]
        public bool UseSortCss { get; set; } = true;

        /// <summary>
        /// Default: th
        /// </summary>
        [Parameter]
        public string HeaderTag { get; set; } = "th";

        /// <summary>
        /// Event to listen to when data is updated
        /// </summary>
        [Parameter]
        public Func<Task>? OnDataUpdated { get; set; }

        /// <summary>
        /// Default:  1
        /// </summary>
        [Range(1, int.MaxValue)]
        [Parameter]
        public int MaxNumberOfSorts { get; set; } = 1;

        internal ColumnSortCollection<TItem> ColumnSorts;
        internal ColumnFilterCollection<TItem> ColumnFilters;
        internal Paginator Paginator;

        protected override async Task OnInitializedAsync()
        {
            Paginator = new Paginator()
            { 
               page = 1
            };
            Paginator.PropertyChanged += Paginator_PropertyChanged;
            await Refresh();
        }

        private async void Paginator_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(Paginator.PaginatorPropertiesChangedList.Contains(e.PropertyName))
                await Refresh();
        }

        private List<TItem> GetData()
        {
            if (SafeList == null)
                return null;

            var query = SafeList.AsQueryable();

            if (ColumnFilters != null)
                query = ColumnFilters.SetFilters(query);

            if (ColumnSorts != null)
                query = ColumnSorts.SetOrderBys(query);

            Paginator.Count = query.Count();

            if (Paginator != null && Paginator.PageSize.HasValue)
                query = query.Skip(Paginator.PageSize.Value * (Paginator.Page - 1)).Take(Paginator.PageSize.Value);

            return query.ToList();
        }

        public void AddFilterComponent(IFilter<TItem> filterComponent)
        {
            if (ColumnFilters == null)
                ColumnFilters = new ColumnFilterCollection<TItem>();
            ColumnFilters.Add(filterComponent);
        }

        public async Task Refresh(bool resetPaging = false)
        {
            if (resetPaging)
                Paginator.Page = 1;

            DisplayList = GetData();
            if (OnDataUpdated != null)
                await OnDataUpdated.Invoke();

            StateHasChanged();
        }

        public Task AddItem(TItem item)
        {
            SafeList.Add(item);
            return Refresh();
        }

        public Task UpdateItem(int index, TItem item)
        {
            var dataIndex = SafeList.IndexOf(DisplayList[index]);
            SafeList[dataIndex] = item;
            return Refresh();
        }

        public Task RemoveItem(int index)
        {
            var item = DisplayList[index];
            SafeList.Remove(item);
            return Refresh();
        }

        public TItem GetItem(int index)
        {
            return DisplayList[index];
        }

        public void Dispose()
        {
            Paginator.PropertyChanged -= Paginator_PropertyChanged;
        }
    }
}
