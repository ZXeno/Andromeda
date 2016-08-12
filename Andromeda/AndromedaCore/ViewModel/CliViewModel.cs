using System;
using System.Windows.Input;
using AndromedaCore.Infrastructure;

namespace AndromedaCore.ViewModel
{
    public class CliViewModel : ViewModelBase, IRequestCloseViewModel
    {
        public event EventHandler RequestClose;
        private void OnRequestClose(EventArgs e)
        {
            RequestClose?.Invoke(this, e);
        }

        private ICommand _okayCmd;
        public ICommand OkayCommand
        {
            get
            {
                if (_okayCmd == null)
                {
                    _okayCmd = new DelegateCommand(param => OkayClose(), param => true);
                }
                return _okayCmd;
            }
        }

        private ICommand _cancelCmd;
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCmd == null)
                {
                    _cancelCmd = new DelegateCommand(param => CancelClose(), param => true);
                }
                return _cancelCmd;
            }
        }

        private string _textboxLabel;
        public string TextBoxLabel
        {
            get { return _textboxLabel; }
            set
            {
                _textboxLabel = value;
                OnPropertyChanged("TextBoxLabel");
            }
        }

        private string _boxContents;
        public string TextBoxContents
        {
            get { return _boxContents; }
            set
            {
                _boxContents = value;
                OnPropertyChanged("TextBoxContents");
            }
        }

        private bool _result;
        public bool Result
        {
            get { return _result; }
            set
            {
                _result = value;
                OnPropertyChanged("Result");
            }
        }

        public CliViewModel(string promptMessage)
        {
            TextBoxLabel = promptMessage;
        }

        public void OkayClose()
        {
            _result = true;
            OnRequestClose(EventArgs.Empty);
        }

        public void CancelClose()
        {
            _result = false;
            OnRequestClose(EventArgs.Empty);
        }
    }
}