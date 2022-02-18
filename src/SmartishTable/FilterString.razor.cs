using Microsoft.AspNetCore.Components;
using SmartishTable.Filters;
using SmartishTable.Interfaces;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace SmartishTable
{
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

        public FilterContext<string> Context { get; private set; }

        public virtual Expression<Func<SmartishTItem, bool>> GetFilter()
        {
            if (string.IsNullOrEmpty(Context.FilterValue))
                return null;

            var param = Expression.Parameter(typeof(SmartishTItem), "w");

            var filterProperty = Expression.Property(param, ExpressionHelper.GetPropertyName(Field));
            var filterParam = Expression.Constant(Context.FilterValue);
            switch (Operator)
            {
                case StringOperators.Contains:
                case StringOperators.StartsWith:
                case StringOperators.EndsWith:
                    var method = typeof(string).GetMethod(Operator.ToString(), new[] { typeof(string), typeof(StringComparison) });
                    var call = Expression.Call(filterProperty, method, filterParam, Expression.Constant(StringComparison.OrdinalIgnoreCase));
                    return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), call), param);
                case StringOperators.Equals:
                    return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.Equal(filterProperty, filterParam)), param);
                case StringOperators.NotEquals:
                    return Expression.Lambda<Func<SmartishTItem, bool>>(Expression.AndAlso(filterProperty.CreateNullChecks(), Expression.NotEqual(filterProperty, filterParam)), param);
            }
            return null;
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
}
