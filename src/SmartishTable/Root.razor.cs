﻿using Microsoft.AspNetCore.Components;
using SmartishTable.Filters;
using SmartishTable.Interfaces;
using SmartishTable.Paging;
using SmartishTable.Sorts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SmartishTable;

public partial class Root<SmartishTItem> : IDisposable
{
    [Parameter]
    public List<SmartishTItem> SafeList { get; set; } = default!;

    internal List<SmartishTItem>? DisplayList { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

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

    internal ColumnSortCollection<SmartishTItem> ColumnSorts = default!;
    internal ColumnFilterCollection<SmartishTItem> ColumnFilters = default!;
    internal Paginator Paginator = default!;
    private bool disposedValue;

    protected override void OnInitialized()
    {
        Paginator = new Paginator()
        { 
           page = 1
        };
        Paginator.PropertyChanged += Paginator_PropertyChanged;
    }

    protected override async Task OnParametersSetAsync()
    {
        await Refresh();
    }

    private async void Paginator_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if(Paginator.PaginatorPropertiesChangedList.Contains(e.PropertyName))
            await Refresh();
    }

    /// <summary>
    /// Gets the settings for smartish table
    /// </summary>
    /// <returns><see cref="SmartishTableSettings"/></returns>
    public SmartishTableSettings GetSettings()
    {
        return new SmartishTableSettings()
        {
            Page = Paginator.Page,
            PageSize = Paginator.PageSize,
            ColumnSorts = ColumnSorts.GetSortSettings()
        };
    }

    /// <summary>
    /// Sets smartish table settings and refreshes
    /// </summary>
    /// <param name="settings"><see cref="SmartishTableSettings"/></param>
    public async Task SetSettings(SmartishTableSettings settings)
    {
        if (settings == null)
            return;

        if (ColumnSorts != null)
            ColumnSorts.SetSortSettings(MaxNumberOfSorts, settings.ColumnSorts);

        if (settings.PageSize.HasValue)
            Paginator.PageSize = settings.PageSize.Value;

        Paginator.Page = settings.Page ?? 1;

        await Refresh();
    }

    private List<SmartishTItem>? GetData()
    {
        if (SafeList == null)
            return null;

        var query = SafeList.AsQueryable();

        if (ColumnFilters != null)
            query = ColumnFilters.SetFilters(query);

        if (ColumnSorts != null)
            query = ColumnSorts.SetOrderBys(query);

        Paginator.Count = query.Count();

        if (Paginator.PageSize.HasValue)
            query = query.Skip(Paginator.PageSize.Value * (Paginator.Page - 1)).Take(Paginator.PageSize.Value);

        return query.ToList();
    }

    /// <summary>
    /// Filters must be added here to function properly
    /// </summary>
    /// <param name="filterComponent"><see cref="IFilter{SmartishTItem}"/></param>
    public void AddFilterComponent(IFilter<SmartishTItem> filterComponent)
    {
        if (ColumnFilters == null)
            ColumnFilters = new ColumnFilterCollection<SmartishTItem>();
        ColumnFilters.Add(filterComponent);
    }

    /// <summary>
    /// Refreshes smartish table by getting data again
    /// </summary>
    /// <param name="resetPaging">resets the page to page 1</param>
    public async Task Refresh(bool resetPaging = false)
    {
        if (resetPaging)
            Paginator.Page = 1;

        DisplayList = GetData();
        if (OnDataUpdated != null)
            await OnDataUpdated.Invoke();

        StateHasChanged();
    }

    /// <summary>
    /// Adds an item safe list
    /// </summary>
    /// <param name="item"><see cref="SmartishTItem"/></param>
    public Task Add(SmartishTItem item)
    {
        SafeList.Add(item);
        return Refresh();
    }

    /// <summary>
    /// Updates an item at specified index
    /// </summary>
    /// <param name="index">index of the displayed item (index is provided by the repeater context)</param>
    /// <param name="item"><see cref="SmartishTItem"/></param>
    public Task UpdateAt(int index, SmartishTItem item)
    {
        var dataIndex = SafeList.IndexOf(DisplayList[index]);
        SafeList[dataIndex] = item;
        return Refresh();
    }

    /// <summary>
    /// Removes the item at specified index
    /// </summary>
    /// <param name="index">index of the displayed item (index is provided by the repeater context)</param>
    public Task RemoveAt(int index)
    {
        var item = DisplayList[index];
        SafeList.Remove(item);
        return Refresh();
    }

    /// <summary>
    /// Get the item at specified index
    /// </summary>
    /// <param name="index">index of the displayed item (index is provided by the repeater context)</param>
    /// <returns><see cref="SmartishTItem"/></returns>
    public SmartishTItem GetAt(int index)
    {
        return DisplayList[index];
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Paginator.PropertyChanged -= Paginator_PropertyChanged;
            }

            disposedValue = true;
        }
    }

    /// <summary>
    /// Disposes SmartishTable
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
