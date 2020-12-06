using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SmartishTable.Samples.Client
{
    public class Counter : INotifyPropertyChanged
    {
        private int count;
        public int Count
        {
            get { return count; }
            set { SetProperty(ref count, value); }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public async void RaisePropertyChange(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
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
