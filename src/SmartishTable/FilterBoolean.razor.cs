using Microsoft.AspNetCore.Components;
using SmartishTable.Filters;
using SmartishTable.Interfaces;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace SmartishTable;

public partial class FilterBoolean<SmartishTItem> : INotifyPropertyChanged, IFilter<SmartishTItem>, IDisposable
{
    [Parameter]
    public RenderFragment<FilterContext<bool?>> ChildContent { get; set; }

    [CascadingParameter(Name = "SmartishTableRoot")]
    public Root<SmartishTItem> Root { get; set; }

    [Parameter]
    public System.Linq.Expressions.Expression<Func<SmartishTItem, object>> Field { get; set; }

    /// <summary>
    /// Default: Equals
    /// </summary>
    [Parameter]
    public BooleanOperators? Operator
    {
        get { return _operator; }
        set { SetProperty(ref _operator, value); }
    }
    private BooleanOperators? _operator = BooleanOperators.Equals;
    private bool disposedValue;

    public FilterContext<bool?> Context { get; private set; }

    public virtual Expression<Func<SmartishTItem, bool>> GetFilter()
    {
        var operatorsThatRequireFilterValue = new[] { BooleanOperators.Equals, BooleanOperators.NotEquals };
        if (Context.FilterValue == null && (!Operator.HasValue || operatorsThatRequireFilterValue.Contains(Operator.Value)))
            return null;

        var fieldType = ExpressionHelper.GetPropertyType(Field).GetNonNullableType();
        var param = Expression.Parameter(typeof(SmartishTItem), "w");
        var filterProperty = Expression.Property(param, ExpressionHelper.GetPropertyName(Field));
        var filterPropertyConverted = Expression.Convert(filterProperty, fieldType);
        switch (Operator)
        {
            case BooleanOperators.Equals:
            case BooleanOperators.NotEquals:
                var value = Convert.ChangeType(Context.FilterValue, fieldType, CultureInfo.InvariantCulture);
                var filterParam = Expression.Constant(value);
                if (Operator == BooleanOperators.Equals)
                    return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.Equal(filterPropertyConverted, filterParam)), param);
                else
                    return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.NotEqual(filterPropertyConverted, filterParam)), param);
            case BooleanOperators.IsTrue:
                return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.IsTrue(filterPropertyConverted)), param);
            case BooleanOperators.IsFalse:
                return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.IsFalse(filterPropertyConverted)), param);
        }
        return null;
    }

    protected override void OnInitialized()
    {
        // **** required for your filter to work ****
        Root.AddFilterComponent(this);

        Context = new FilterContext<bool?>();
        Context.PropertyChanged += Context_PropertyChanged;
    }

    private async void Context_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        await Root.Refresh(true);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public async void RaisePropertyChange(string propertyname)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));

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

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Context.PropertyChanged -= Context_PropertyChanged;
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
