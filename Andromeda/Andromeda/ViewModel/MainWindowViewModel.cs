using System;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Andromeda.Command;
using Andromeda.Model;
using Andromeda.MVVM;
using Action = Andromeda.Command.Action;

namespace Andromeda.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<Action> ActionsList { get; private set; }
        private ObservableCollection<ViewModelBase> _viewModels; 
        public ObservableCollection<ViewModelBase> ViewModels
        {
            get
            {
                if (ViewModels == null)
                {
                    _viewModels = new ObservableCollection<ViewModelBase>();
                }

                return _viewModels;
            }
        }

        private string _domain;
        public string Domain
        {
            get { return _domain; }
            set
            {
                _domain = value;
                OnPropertyChanged("Domain");
            }
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged("Username");
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged("Password");
                Program.CredentialManager.SetCredentials(Domain, Username, Program.CredentialManager.BuildSecureString(_password));
            }
        }

        private string _deviceListString;
        public string DeviceListString
        {
            get { return _deviceListString; }
            set
            {
                _deviceListString = value;
                OnPropertyChanged("DeviceListString");
            }
        }

        private Action _selectedAction;
        public Action SelectedAction
        {
            get { return _selectedAction; }
            set
            {
                _selectedAction = value;
                OnPropertyChanged("SelectedAction");
                RunCommand = new DelegateCommand(param => RunCommandExecute(), param => RunCommandCanExecute());
            }
        }

        private ICommand _runCmd;
        public ICommand RunCommand
        {
            get
            {
                if (_runCmd == null)
                {
                    RunCommand = new DelegateCommand(param => RunCommandExecute(), param => RunCommandCanExecute());
                }
                return _runCmd;
            }
            set
            {
                _runCmd = value;
                OnPropertyChanged("RunCommand");
            }
        }

        public MainWindowViewModel()
        {
            ActionsList = new ObservableCollection<Action>
            {
                new PingTest(),
                new GetLoggedOnUser(),
                new InstallTightVNC(),
                new RemoveTightVNC(),
                new GetPHPrintFile(),
                new RunRemoteCommand(),
                new FixCEDeviceID(),
                new ForceReboot(),
                new ForceLogOff(),
                new UninstallXceleraMonitor(),
                new AppDeploymentSchedule()
            };

            _viewModels = new ObservableCollection<ViewModelBase>();
            _viewModels.Add(new ResultConsole());

            DeviceListString = "Wxxxxxx";
            Domain = Environment.UserDomainName;
        }

        protected override void OnDispose()
        {
            ActionsList.Clear();
            ViewModels.Clear(); 
        }

        public void RunCommandExecute()
        {
            if (SelectedAction != null)
            {
                SelectedAction.RunCommand(DeviceListString);
            }
        }

        public bool RunCommandCanExecute()
        {
            if (SelectedAction == null)
            {
                return false;
            }

            return true;
        }
    }
}