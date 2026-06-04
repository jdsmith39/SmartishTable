using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
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

  /// <summary>
  /// Contains the filtered/sorted displaylist.  Only contains current displayed page.
  /// </summary>
  public List<SmartishTItem>? DisplayList { get; internal set; }

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
  /// Use to change what the "Sort" component builds out
  /// </summary>
  [Parameter]
  public string HeaderTag { get; set; } = "th";

  /// <summary>
  /// Event to listen to when data is updated and sends current configuration
  /// </summary>
  [Parameter]
  public EventCallback<SmartishTableSettings> OnDataUpdated { get; set; }

  /// <summary>
  /// Default:  1
  /// </summary>
  [Range(1, int.MaxValue)]
  [Parameter]
  public int MaxNumberOfSorts { get; set; } = 1;

  /// <summary>
  /// Initial settings
  /// </summary>
  [Parameter]
  public SmartishTableSettings? InitialSettings { get; set; }

  internal ColumnSortCollection<SmartishTItem> ColumnSorts = default!;
  internal ColumnFilterCollection<SmartishTItem> ColumnFilters = default!;
  internal Paginator Paginator = default!;
  private bool maxNumberOfSortsDecreased = false;
  private bool disposedValue;
  private readonly Type rootType = typeof(Root<SmartishTItem>);
  private static readonly string[] ReloadTriggerParameters = new string[]
  {
        nameof(SafeList),
        nameof(MaxNumberOfSorts),
  };

  protected override void OnInitialized()
  {
    logger.LogDebug($"{nameof(OnInitialized)} called.");
    Paginator = new Paginator()
    {
      page = 1,
      // set to 0 initially so all data won't be rendered before the real pageSize is set within the Paginator context.
      pageSize = 0
    };
    Paginator.PropertyChanged += Paginator_PropertyChanged;

    logger.LogDebug($"{nameof(OnInitialized)} end.");
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    logger.LogDebug($"{nameof(SetParametersAsync)} called.");
    var shouldRender = false;
    var p = parameters.ToDictionary();

    foreach (var item in ReloadTriggerParameters)
    {
      if (p.ContainsKey(item))
      {
        var newValue = p[item]?.GetHashCode();
        var oldValue = rootType.GetProperty(item)!.GetValue(this)?.GetHashCode();

        shouldRender = newValue != oldValue;

        if (item == nameof(MaxNumberOfSorts))
        {
          maxNumberOfSortsDecreased = MaxNumberOfSorts > (int)p[item];
        }
      }

      if (shouldRender)
      {
        break;
      }
    }

    await base.SetParametersAsync(parameters);

    if (shouldRender)
      await Refresh();

    logger.LogDebug($"{nameof(SetParametersAsync)} call ended. Should Reload: {shouldRender}");
  }

  private int? oldDisplayListHashCode;
  private bool shouldRender;
  protected override bool ShouldRender()
  {
    var newHashCode = DisplayList?.GetHashCode();

    if (oldDisplayListHashCode != newHashCode)
    {
      oldDisplayListHashCode = newHashCode;
      shouldRender = true;
    }

    logger.LogDebug($"{nameof(ShouldRender)} call ended. Should Render: {shouldRender}");
    return shouldRender;
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    logger.LogDebug($"{nameof(OnAfterRenderAsync)} called.  InitialSettings != null && firstRender: {(InitialSettings != null && firstRender)}");
    if (InitialSettings != null && firstRender)
    {
      await SetSettings(InitialSettings, true);
    }
    // if PageSize is still 0 then it hasn't be set by the developer, set it to null to show everything.
    if (Paginator.PageSize == 0)
      Paginator.PageSize = null;
    logger.LogDebug($"{nameof(OnAfterRenderAsync)} ended.");
  }

  private async void Paginator_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
  {
    logger.LogDebug($"{nameof(Paginator_PropertyChanged)} called. Property Name: {e.PropertyName}");
    if (Paginator.PaginatorPropertiesChangedList.Contains(e.PropertyName))
      await Refresh();

    logger.LogDebug($"{nameof(Paginator_PropertyChanged)} ended.");
  }

  /// <summary>
  /// Gets the settings for smartish table
  /// </summary>
  /// <returns><see cref="SmartishTableSettings"/></returns>
  public SmartishTableSettings GetSettings()
  {
    logger.LogDebug($"{nameof(GetSettings)} called.");
    return new SmartishTableSettings()
    {
      Page = Paginator.Page,
      PageSize = Paginator.PageSize,
      ColumnSorts = ColumnSorts?.GetSortSettings()
    };
  }

  /// <summary>
  /// Sets smartish table settings and refreshes
  /// </summary>
  /// <param name="settings"><see cref="SmartishTableSettings"/></param>
  public async Task SetSettings(SmartishTableSettings settings)
  {
    logger.LogDebug($"{nameof(SetSettings)} called.");
    await SetSettings(settings, true);
    logger.LogDebug($"{nameof(SetSettings)} ended.");
  }

  private async Task SetSettings(SmartishTableSettings settings, bool refresh)
  {
    if (settings == null)
      return;

    if (ColumnSorts != null)
      ColumnSorts.SetSortSettings(MaxNumberOfSorts, settings.ColumnSorts);

    if (settings.PageSize.HasValue)
      Paginator.pageSize = settings.PageSize.Value;

    Paginator.page = settings.Page ?? 1;

    if (refresh)
      await Refresh();
  }

  private List<SmartishTItem>? GetData()
  {
    logger.LogDebug($"{nameof(GetData)} callled.");
    if (SafeList == null)
      return null;

    var query = SafeList.AsQueryable();

    if (ColumnFilters != null)
      query = ColumnFilters.SetFilters(query);

    if (ColumnSorts != null)
    {
      if (maxNumberOfSortsDecreased)
      {
        ColumnSorts.RemoveHighestSortOrders(MaxNumberOfSorts);
        maxNumberOfSortsDecreased = false;
      }
      query = ColumnSorts.SetOrderBys(query);
    }

    Paginator.Count = query.Count();

    if (Paginator.PageSize.HasValue)
      query = query.Skip(Paginator.PageSize.Value * (Paginator.Page - 1)).Take(Paginator.PageSize.Value);

    logger.LogDebug($"{nameof(GetData)} ended.");
    return query.ToList();
  }

  /// <summary>
  /// Filters must be added here to function properly
  /// </summary>
  /// <param name="filterComponent"><see cref="IFilter{SmartishTItem}"/></param>
  public void AddFilterComponent(IFilter<SmartishTItem> filterComponent)
  {
    logger.LogDebug($"{nameof(AddFilterComponent)} called.");
    if (ColumnFilters == null)
      ColumnFilters = new ColumnFilterCollection<SmartishTItem>();
    ColumnFilters.Add(filterComponent);
    logger.LogDebug($"{nameof(AddFilterComponent)} ended.");
  }

  /// <summary>
  /// Refreshes smartish table by getting data again
  /// </summary>
  /// <param name="resetPaging">resets the page to page 1</param>
  public async Task Refresh(bool resetPaging = false)
  {
    logger.LogDebug($"{nameof(Refresh)} called.  resetPaging: {{resetPaging}}", resetPaging);
    if (resetPaging)
      Paginator.Page = 1;

    DisplayList = GetData();

    if (OnDataUpdated.HasDelegate)
      await OnDataUpdated.InvokeAsync(GetSettings());

    StateHasChanged();

    logger.LogDebug($"{nameof(Refresh)} ended.");
  }

  /// <summary>
  /// Adds an item safe list
  /// </summary>
  /// <param name="item"><see cref="SmartishTItem"/></param>
  /// <param name="refresh">refresh smartish table after performing action</param>
  public Task Add(SmartishTItem item, bool refresh = true)
  {
    SafeList.Add(item);
    if (refresh)
      return Refresh();

    return Task.CompletedTask;
  }

  /// <summary>
  /// Adds an item range to the safe list
  /// </summary>
  /// <param name="items"><see cref="List{SmartishTItem}"/></param>
  /// <param name="refresh">refresh smartish table after performing action</param>
  public Task AddRange(IEnumerable<SmartishTItem> items, bool refresh = true)
  {
    SafeList.AddRange(items);
    if (refresh)
      return Refresh();

    return Task.CompletedTask;
  }

  /// <summary>
  /// Updates an item at specified index
  /// </summary>
  /// <param name="index">index of the displayed item (index is provided by the repeater context)</param>
  /// <param name="item"><see cref="SmartishTItem"/></param>
  /// <param name="refresh">refresh smartish table after performing action</param>
  public Task UpdateAt(int index, SmartishTItem item, bool refresh = true)
  {
    var dataIndex = SafeList.IndexOf(DisplayList[index]);
    SafeList[dataIndex] = item;
    if (refresh)
      return Refresh();

    return Task.CompletedTask;
  }

  /// <summary>
  /// Removes the item at specified index
  /// </summary>
  /// <param name="index">index of the displayed item (index is provided by the repeater context)</param>
  /// <param name="refresh">refresh smartish table after performing action</param>
  public Task RemoveAt(int index, bool refresh = true)
  {
    var item = DisplayList[index];
    SafeList.Remove(item);
    if (refresh)
      return Refresh();

    return Task.CompletedTask;
  }

  /// <summary>
  /// Get the item at specified index
  /// </summary>
  /// <param name="index">index of the displayed item (index is provided by the repeater context)</param>
  /// <returns><see cref="SmartishTItem"/></returns>
  public SmartishTItem? GetAt(int index)
  {
    if (DisplayList == null)
      return default;
    return DisplayList[index];
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!disposedValue)
    {
      if (disposing)
      {
        Paginator.PropertyChanged -= Paginator_PropertyChanged!;
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
