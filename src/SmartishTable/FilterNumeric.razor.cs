using Microsoft.AspNetCore.Components;
using SmartishTable.Filters;
using SmartishTable.Helpers;
using SmartishTable.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SmartishTable
{
    public partial class FilterNumeric<TItem, FilterType> : INotifyPropertyChanged, IFilter<TItem>, IDisposable
    {
        [Parameter]
        public RenderFragment<FilterContext<FilterType>> ChildContent { get; set; }

        [CascadingParameter(Name = "SmartishTableRoot")]
        public Root<TItem> Root { get; set; }

        [Parameter]
        public System.Linq.Expressions.Expression<Func<TItem, object>> Field { get; set; }

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

        public Expression<Func<TItem, bool>> GetFilter()
        {
            if (Context.FilterValue == null)
                return null;

            var fieldType = ExpressionHelper.GetPropertyType(Field).GetNonNullableType();
            var param = Expression.Parameter(typeof(TItem), "w");
            var filterProperty = Expression.Property(param, ExpressionHelper.GetPropertyName(Field));
            var filterPropertyConverted = Expression.Convert(filterProperty, fieldType);
            var value = Convert.ChangeType(Context.FilterValue, fieldType, CultureInfo.InvariantCulture);
            var filterParam = Expression.Constant(value);
            var nullExpression = Expression.Constant(null);
            switch (Operator)
            {
                case NumericOperators.Equals:
                    return Expression.Lambda<Func<TItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.Equal(filterPropertyConverted, filterParam)), param);
                case NumericOperators.NotEquals:
                    return Expression.Lambda<Func<TItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.NotEqual(filterPropertyConverted, filterParam)), param);
                case NumericOperators.GreaterThan:
                    return Expression.Lambda<Func<TItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.GreaterThan(filterPropertyConverted, filterParam)), param);
                case NumericOperators.GreaterThanOrEqual:
                    return Expression.Lambda<Func<TItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.GreaterThanOrEqual(filterPropertyConverted, filterParam)), param);
                case NumericOperators.LessThan:
                    return Expression.Lambda<Func<TItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.LessThan(filterPropertyConverted, filterParam)), param);
                case NumericOperators.LessThanOrEqual:
                    return Expression.Lambda<Func<TItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.LessThanOrEqual(filterPropertyConverted, filterParam)), param);
            }
            return null;
        }

        protected override void OnInitialized()
        {
            if (!typeof(FilterType).IsNumeric())
                throw new Exception($"{nameof(FilterType)} must be a numeric type.");

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
}
