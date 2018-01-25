using System;
using System.Windows.Input;
using AndromedaCore.Infrastructure;

namespace AndromedaCore.ViewModel
{
    public class DialogBoxViewModel : RequestCloseViewModel
    {
        private ICommand _okayCmd;
        public ICommand OkayCommand
        {
            get { return _okayCmd ?? (_okayCmd = new DelegateCommand(param => OkayClose(), param => true)); }
        }

        private ICommand _cancelCmd;

        public ICommand CancelCommand
        {
            get { return _cancelCmd ?? (_cancelCmd = new DelegateCommand(param => CancelClose(), param => true)); }
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

        public DialogBoxViewModel(string promptMessage)
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