using System;
using System.Windows.Input;
using AndromedaCore.Infrastructure;
using AndromedaCore.ViewModel;
using Microsoft.Win32;

namespace AndromedaActions.ViewModel
{
    public class FileCopyPromptViewModel : RequestCloseViewModel
    {
        private string _filePath;
        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged("FilePath");
            }
        }

        private string _destPath;
        public string DestinationPath
        {
            get => _destPath;
            set
            {
                _destPath = value;
                OnPropertyChanged("DestinationPath");
            }
        }

        private ICommand _openFileCommand;
        public ICommand OpenFileCommand
        {
            get
            {
                if (_openFileCommand == null)
                {
                    _openFileCommand = new DelegateCommand(param => OpenFile(), param => true);
                }
                return _openFileCommand;
            }
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

        private bool _createDestination;
        public bool CreateDestination
        {
            get => _createDestination;
            set
            {
                _createDestination = value;
                OnPropertyChanged("CreateDestination");
            }
        }

        private bool _overwrite;
        public bool Overwrite
        {
            get => _overwrite;
            set
            {
                _overwrite = value;
                OnPropertyChanged("Overwrite");
            }
        }

        private bool _result;
        public bool Result
        {
            get => _result;
            set
            {
                _result = value;
                OnPropertyChanged("Result");
            }
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

        public void OpenFile()
        {
            var fileDialog = new OpenFileDialog {Multiselect = false};
            fileDialog.ShowDialog();

            FilePath = fileDialog.FileName;
        }
    }
}