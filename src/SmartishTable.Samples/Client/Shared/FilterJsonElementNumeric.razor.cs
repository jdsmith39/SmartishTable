using Microsoft.AspNetCore.Components;
using SmartishTable.Filters;
using SmartishTable.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace SmartishTable.Samples.Client.Shared
{
    public partial class FilterJsonElementNumeric<FilterType> : INotifyPropertyChanged, IFilter<Dictionary<string, JsonElement>>, IDisposable
    {
        [Parameter]
        public RenderFragment<FilterContext<FilterType>> ChildContent { get; set; }

        [CascadingParameter(Name = "SmartishTableRoot")]
        public Root<Dictionary<string, JsonElement>> Root { get; set; }

        [Parameter]
        public string PropertyName { get; set; }

        /// <summary>
        /// Default: "="
        /// </summary>
        [Parameter]
        public NumericOperators Operator
        {
            get { return _operator; }
            set { SetProperty(ref _operator, value); }
        }
        private NumericOperators _operator = NumericOperators.Equals;

        public FilterContext<string> Context { get; private set; }

        public virtual Expression<Func<Dictionary<string, JsonElement>, bool>> GetFilter()
        {
            if (string.IsNullOrEmpty(Context.FilterValue))
                return null;

            switch (Operator)
            {
                case NumericOperators.Equals:
                    return x => x[PropertyName].GetString().Contains(Context.FilterValue, StringComparison.InvariantCultureIgnoreCase);
                case NumericOperators.NotEquals:
                case NumericOperators.GreaterThan:
                case NumericOperators.GreaterThanOrEqual:
                case NumericOperators.LessThan:
                case NumericOperators.LessThanOrEqual:
                    break;
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

        private async void Context_PropertyChanged(object sender, PropertyChangedEventArgs e)
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

            if (Root != null)
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
