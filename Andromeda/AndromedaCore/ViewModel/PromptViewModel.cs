using System;
using System.Windows.Input;
using AndromedaCore.Infrastructure;

namespace AndromedaCore.ViewModel
{
    public class PromptViewModel : RequestCloseViewModel
    {
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
            get => _textboxLabel;
            set
            {
                _textboxLabel = value;
                OnPropertyChanged();
            }
        }

        private string _boxContents;
        public string TextBoxContents
        {
            get => _boxContents;
            set
            {
                _boxContents = value;
                OnPropertyChanged();
            }
        }

        private bool _result;
        public bool Result
        {
            get => _result;
            set
            {
                _result = value;
                OnPropertyChanged();
            }
        }

        public PromptViewModel(string promptMessage)
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