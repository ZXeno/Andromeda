using System;
using System.Windows;
using System.Windows.Input;
using Andromeda.MVVM;

namespace Andromeda.ViewModel
{
    public class CliViewModel : ViewModelBase
    {
        private CLI_Prompt newPrompt;

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

        private string boxContents;
        private bool _result;

        public string TextBoxContents
        {
            get { return boxContents; }
            set
            {
                boxContents = value;
                OnPropertyChanged("TextBoxContents");
            }
        }

        public bool Result
        {
            get { return _result; }
            set
            {
                _result = value;
                OnPropertyChanged("Result");
            }
        }

        public void OpenNewPrompt()
        {
            newPrompt = new CLI_Prompt
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = App.Current.MainWindow
            };
            newPrompt.ShowDialog();
        }

        public void OkayClose()
        {
            _result = true;
            newPrompt.Close();
            newPrompt = null;
        }

        public void CancelClose()
        {
            _result = false;
            newPrompt.Close();
            newPrompt = null;
        }


        protected override void OnDispose()
        {
            newPrompt = null;
            boxContents = "";
        }
    }
}