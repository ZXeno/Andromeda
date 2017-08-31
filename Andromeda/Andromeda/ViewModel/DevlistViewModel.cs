using System;
using System.Windows.Input;
using AndromedaCore.Infrastructure;
using AndromedaCore.ViewModel;

namespace Andromeda.ViewModel
{
    public class DevlistViewModel : RequestCloseViewModel
    {
        private string _rawDevlistString;
        public string DevlistString
        {
            get => _rawDevlistString;
            set
            {
                _rawDevlistString = value;
                OnPropertyChanged();
            }
        }

        private ICommand _close;
        public ICommand CloseCommand
        {
            get
            {
                if (_close == null)
                {
                    _close = new DelegateCommand(param => Close(), param => true);
                }
                return _close;
            }
        }

        public DevlistViewModel(string deviceListString)
        {
            DevlistString = deviceListString;
        }

        public void Close()
        {
            OnRequestClose(EventArgs.Empty);
        }
    }
}