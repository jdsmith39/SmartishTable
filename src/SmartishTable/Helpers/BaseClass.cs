using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SmartishTable.Helpers
{
    public abstract class BaseClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChange(string propertyname)
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
