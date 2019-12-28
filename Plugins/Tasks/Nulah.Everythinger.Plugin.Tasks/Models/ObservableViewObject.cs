using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Nulah.Everythinger.Plugins.Tasks
{
    public class ObservableViewObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
