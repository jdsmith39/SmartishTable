using Microsoft.AspNetCore.Components;
using SmartishTable.Filters;
using SmartishTable.Interfaces;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace SmartishTable
{
    public partial class FilterDateTime<SmartishTItem> : INotifyPropertyChanged, IFilter<SmartishTItem>, IDisposable
    {
        [Parameter]
        public RenderFragment<FilterContext<DateTime?>> ChildContent { get; set; }

        [CascadingParameter(Name = "SmartishTableRoot")]
        public Root<SmartishTItem> Root { get; set; }

        [Parameter]
        public System.Linq.Expressions.Expression<Func<SmartishTItem, object>> Field { get; set; }

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

        public FilterContext<DateTime?> Context { get; private set; }

        public virtual Expression<Func<SmartishTItem, bool>> GetFilter()
        {
            if (Context.FilterValue == null)
                return null;

            var fieldType = ExpressionHelper.GetPropertyType(Field).GetNonNullableType();
            var param = Expression.Parameter(typeof(SmartishTItem), "w");
            var filterProperty = Expression.Property(param, ExpressionHelper.GetPropertyName(Field));
            var filterPropertyConverted = Expression.Convert(filterProperty, fieldType);
            var value = Convert.ChangeType(Context.FilterValue, fieldType, CultureInfo.InvariantCulture);
            var filterParam = Expression.Constant(value);
            var nullExpression = Expression.Constant(null);
            switch (Operator)
            {
                case DateTimeOperators.Equals:
                    return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.Equal(filterPropertyConverted, filterParam)), param);
                case DateTimeOperators.NotEquals:
                    return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.NotEqual(filterPropertyConverted, filterParam)), param);
                case DateTimeOperators.GreaterThan:
                    return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.GreaterThan(filterPropertyConverted, filterParam)), param);
                case DateTimeOperators.GreaterThanOrEqual:
                    return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.GreaterThanOrEqual(filterPropertyConverted, filterParam)), param);
                case DateTimeOperators.LessThan:
                    return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.LessThan(filterPropertyConverted, filterParam)), param);
                case DateTimeOperators.LessThanOrEqual:
                    return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.LessThanOrEqual(filterPropertyConverted, filterParam)), param);
            }
            return null;
        }

        protected override void OnInitialized()
        {
            // **** required for your filter to work ****
            Root.AddFilterComponent(this);

            Context = new FilterContext<DateTime?>();
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
