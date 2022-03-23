using Microsoft.AspNetCore.Components;
using SmartishTable.Filters;
using SmartishTable.Interfaces;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace SmartishTable;

public partial class FilterDatesTimes<SmartishTItem, FilterType> : INotifyPropertyChanged, IFilter<SmartishTItem>, IDisposable
{
    [Parameter]
    public RenderFragment<FilterContext<FilterType>> ChildContent { get; set; }

    [CascadingParameter(Name = "SmartishTableRoot")]
    public Root<SmartishTItem> Root { get; set; }

    [Parameter]
    public Expression<Func<SmartishTItem, object>> Field { get; set; }

    /// <summary>
    /// Default: Equals
    /// </summary>
    [Parameter]
    public DateTimeOperators Operator
    {
        get { return _operator; }
        set { SetProperty(ref _operator, value); }
    }
    private DateTimeOperators _operator = DateTimeOperators.Equals;

    public FilterContext<FilterType> Context { get; private set; }

    public virtual Expression<Func<SmartishTItem, bool>> GetFilter()
    {
        if (Context.FilterValue == null)
            return null;

        var fieldType = ExpressionHelper.GetPropertyType(Field).GetNonNullableType();
        var propertyPath = Field.GetPropertyName(fieldType);
        var paramExp = Expression.Parameter(typeof(SmartishTItem), "w");
        var filterProperty = ExpressionHelper.GetLastMemberExpression(propertyPath, paramExp);
        var filterPropertyConverted = Expression.Convert(filterProperty, fieldType);
        var value = Convert.ChangeType(Context.FilterValue, fieldType, CultureInfo.InvariantCulture);
        var filterParam = Expression.Constant(value);
        switch (Operator)
        {
            case DateTimeOperators.Equals:
                return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.Equal(filterPropertyConverted, filterParam)), paramExp);
            case DateTimeOperators.NotEquals:
                return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.NotEqual(filterPropertyConverted, filterParam)), paramExp);
            case DateTimeOperators.GreaterThan:
                return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.GreaterThan(filterPropertyConverted, filterParam)), paramExp);
            case DateTimeOperators.GreaterThanOrEqual:
                return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.GreaterThanOrEqual(filterPropertyConverted, filterParam)), paramExp);
            case DateTimeOperators.LessThan:
                return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.LessThan(filterPropertyConverted, filterParam)), paramExp);
            case DateTimeOperators.LessThanOrEqual:
                return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.LessThanOrEqual(filterPropertyConverted, filterParam)), paramExp);
        }
        return null;
    }

    protected override void OnInitialized()
    {
        // **** required for your filter to work ****
        Root.AddFilterComponent(this);

        Context = new FilterContext<FilterType>();
        Context.PropertyChanged += Context_PropertyChanged;
    }

    private async void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        await Root.Refresh(true);
    }

    public void Dispose()
    {
        Context.PropertyChanged -= Context_PropertyChanged;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public async void RaisePropertyChange(string propertyname)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
        }

        if (propertyname == nameof(Operator) && Root != null)
            await Root.Refresh();
    }

    protected bool SetProperty<T>(ref T prop, T value, [CallerMemberName] string propertyName = null)
    {
        if (object.Equals(prop, value)) return false;
        prop = value;
        this.RaisePropertyChange(propertyName);
        return true;
    }
}
