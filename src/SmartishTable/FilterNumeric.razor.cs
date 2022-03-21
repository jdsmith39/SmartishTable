using Microsoft.AspNetCore.Components;
using SmartishTable.Filters;
using SmartishTable.Interfaces;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace SmartishTable;

public partial class FilterNumeric<SmartishTItem, FilterType> : INotifyPropertyChanged, IFilter<SmartishTItem>, IDisposable
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
    public NumericOperators Operator
    {
        get { return _operator; }
        set { SetProperty(ref _operator, value); }
    }
    private NumericOperators _operator = NumericOperators.Equals;

    public FilterContext<FilterType> Context { get; private set; }

    public virtual Expression<Func<SmartishTItem, bool>> GetFilter()
    {
        if (Context.FilterValue == null)
            return null;
        Console.WriteLine(Field.NodeType);
        var fieldType = ExpressionHelper.GetPropertyType(Field).GetNonNullableType();
        Console.WriteLine("FieldType:  " + fieldType.FullName);
        var propertyPath = Field.GetMemberExpression().GetPropertyName(fieldType);
        Console.WriteLine("FullPath:  " + propertyPath);
        var paramExp = Expression.Parameter(typeof(SmartishTItem), "w");
        // The property name might be a nested expression like a.b.c
        var path = propertyPath.Split('.');
        Expression lastMember = paramExp;
        
        foreach (var p in path)
        {
            if (lastMember == null)
            {
                lastMember = Expression.Property(paramExp, p);
            }
            else
            {
                lastMember = Expression.Property(lastMember, p);
            }
        }

        var filterPropertyExpression = ExpressionHelper.CreatePropertyExpression<SmartishTItem>(propertyPath);
        //var filterProperty = ExpressionHelper.InnerRecursiveGet()
        //var filterProperty = Expression.Property(param, ExpressionHelper.GetPropertyName(Field));
        var filterProperty = lastMember;// Expression.Property(filterPropertyExpression, ExpressionHelper.GetPropertyName(Field));
        var filterPropertyConverted = Expression.Convert(filterProperty, fieldType);
        var value = Convert.ChangeType(Context.FilterValue, fieldType, CultureInfo.InvariantCulture);
        var filterParam = Expression.Constant(value);
        switch (Operator)
        {
            case NumericOperators.Equals:
                return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.Equal(filterPropertyConverted, filterParam)), paramExp);
            case NumericOperators.NotEquals:
                return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.NotEqual(filterPropertyConverted, filterParam)), paramExp);
            case NumericOperators.GreaterThan:
                return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.GreaterThan(filterPropertyConverted, filterParam)), paramExp);
            case NumericOperators.GreaterThanOrEqual:
                return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.GreaterThanOrEqual(filterPropertyConverted, filterParam)), paramExp);
            case NumericOperators.LessThan:
                return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.LessThan(filterPropertyConverted, filterParam)), paramExp);
            case NumericOperators.LessThanOrEqual:
                return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.LessThanOrEqual(filterPropertyConverted, filterParam)), paramExp);
        }
        return null;
    }

    protected override void OnInitialized()
    {
        if (!typeof(FilterType).IsNumeric())
            throw new Exception($"{nameof(FilterType)} must be a numeric type.");

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
