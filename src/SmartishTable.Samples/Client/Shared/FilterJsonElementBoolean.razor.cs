using Microsoft.AspNetCore.Components;
using SmartishTable.Filters;
using SmartishTable.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace SmartishTable.Samples.Client.Shared
{
    public partial class FilterJsonElementBoolean : INotifyPropertyChanged, IFilter<Dictionary<string, JsonElement>>, IDisposable
    {
        [Parameter]
        public RenderFragment<FilterContext<bool?>> ChildContent { get; set; }

        [CascadingParameter(Name = "SmartishTableRoot")]
        public Root<Dictionary<string, JsonElement>> Root { get; set; }

        [Parameter]
        public string PropertyName { get; set; }

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

        public Expression<Func<Dictionary<string, JsonElement>, bool>> GetFilter()
        {
            var operatorsThatRequireFilterValue = new[] { BooleanOperators.Equals, BooleanOperators.NotEquals };
            if (Context.FilterValue == null && (!Operator.HasValue || operatorsThatRequireFilterValue.Contains(Operator.Value)))
                return null;

            switch (Operator)
            {
                case BooleanOperators.IsTrue:
                    return x => x[PropertyName].ValueKind == JsonValueKind.True;
                case BooleanOperators.IsFalse:
                    return x => x[PropertyName].ValueKind == JsonValueKind.False;
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
}
