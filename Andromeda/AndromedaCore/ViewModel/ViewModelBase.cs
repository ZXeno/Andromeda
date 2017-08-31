using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AndromedaCore.ViewModel
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        public bool MinimizeWindowToTray { get; protected set; }

        protected ViewModelBase() { }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            this.OnDispose();
        }

        protected virtual void OnDispose()
        {

        }
    }
}