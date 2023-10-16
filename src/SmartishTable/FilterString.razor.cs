using Microsoft.AspNetCore.Components;
using SmartishTable.Filters;
using SmartishTable.Interfaces;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace SmartishTable;

public partial class FilterString<SmartishTItem> : INotifyPropertyChanged, IFilter<SmartishTItem>, IDisposable
{
    [Parameter]
    public RenderFragment<FilterContext<string>> ChildContent { get; set; }

    [CascadingParameter(Name = "SmartishTableRoot")]
    public Root<SmartishTItem> Root { get; set; }

    [Parameter]
    public Expression<Func<SmartishTItem, object>> Field { get; set; }

    /// <summary>
    /// Default: Contains
    /// </summary>
    [Parameter]
    public StringOperators Operator
    {
        get { return _operator; }
        set { SetProperty(ref _operator, value); }
    }
    private StringOperators _operator = StringOperators.Contains;

    [Parameter]
    public bool IsCaseSensitive { get; set; }

    /// <summary>
    /// This will ignore the Operator parameter if set
    /// </summary>
    [Parameter]
    public Func<string, Expression<Func<SmartishTItem, bool>>>? FilterOverride { get; set; }
    
    public FilterContext<string> Context { get; private set; }

    public virtual Expression<Func<SmartishTItem, bool>> GetFilter()
    {
        if (string.IsNullOrEmpty(Context.FilterValue))
            return null;

        if (FilterOverride != null)
            return FilterOverride(Context.FilterValue);

        var fieldType = ExpressionHelper.GetPropertyType(Field).GetNonNullableType();
        var paramExp = Expression.Parameter(typeof(SmartishTItem), "w");
        var propertyPath = Field.GetPropertyName(fieldType);
        var filterProperty = ExpressionHelper.GetLastMemberExpression(propertyPath, paramExp);
        var filterParam = Expression.Constant(Context.FilterValue);

        var methodCall = GetMethodCallByOperator(filterProperty, filterParam, IsCaseSensitive);

        if (methodCall == null)
            return null;

        return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), methodCall), paramExp);
    }

    private Expression GetMethodCallByOperator(Expression filterProperty, ConstantExpression filterParam, bool isCaseSensitive)
    {
        switch (Operator)
        {
            case StringOperators.Contains:
            case StringOperators.StartsWith:
            case StringOperators.EndsWith:
            case StringOperators.Equals:
                var method = typeof(string).GetMethod(Operator.ToString(), new[] { typeof(string), typeof(StringComparison) });
                return Expression.Call(filterProperty, method, filterParam, Expression.Constant(GetCaseSensitiveComparisonType(isCaseSensitive)));

            case StringOperators.NotEquals:
                var notEqualsMethod = typeof(string).GetMethod(StringOperators.Equals.ToString(), new[] { typeof(string), typeof(StringComparison) });
                var equalsCall = Expression.Call(filterProperty, notEqualsMethod, filterParam, Expression.Constant(GetCaseSensitiveComparisonType(isCaseSensitive)));
                return Expression.Not(equalsCall);
        }

        return null;
    }

    private StringComparison GetCaseSensitiveComparisonType(bool isCaseSensitive)
    {
        return isCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
    }

    protected override void OnInitialized()
    {
        // **** required for your filter to work ****
        Root.AddFilterComponent(this);

        Context = new FilterContext<string>();
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
