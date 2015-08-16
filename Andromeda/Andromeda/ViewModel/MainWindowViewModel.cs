using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Andromeda.Command;
using Andromeda.Model;
using Andromeda.MVVM;
using Action = Andromeda.Command.Action;

namespace Andromeda.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        public delegate void ActionsStarted(bool justStarted);
        public static event ActionsStarted ActionStart;
        public void OnActionStarted(bool justStarted)
        {
            if (ActionStart != null)
            {
                ActionStart(justStarted);
            }
        }

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

        private string _runButtonText;
        public string RunButtonText
        {
            get { return _runButtonText; }
            set
            {
                _runButtonText = value;
                OnPropertyChanged("RunButtonText");
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
                OnPropertyChanged("Password");
                Program.CredentialManager.SetCredentials(Domain, Username, Program.CredentialManager.BuildSecureString(value));                
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

        private bool _actionRunning;
        public bool ActionRunning
        {
            get { return _actionRunning; }
            set
            {
                _actionRunning = value;
                OnPropertyChanged("ActionRunning");
            }
        }

        private ProgressData _progressData;
        public ProgressData ProgressData
        {
            get { return _progressData; }
            set
            {
                _progressData = value;
                OnPropertyChanged("ProgressData");
                OnPropertyChanged("ProgressBarVisible");
            }
        }

        public int ProgressTotal
        {
            get { return _progressData.TotalCount; }
            set
            {
                _progressData.TotalCount = value;
                OnPropertyChanged("ProgressTotal");
                OnPropertyChanged("ProgressBarVisible");
            }
        }

        public int ProgressCurrent
        {
            get { return _progressData.Current; }
            set
            {
                _progressData.Current = value;
                OnPropertyChanged("ProgressCurrent");
                OnPropertyChanged("ProgressBarVisible");
            }
        }

        public Visibility ProgressBarVisible
        {
            get { return (ProgressCurrent < ProgressTotal && ActionRunning) ? Visibility.Visible : Visibility.Collapsed; }
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
                new SccmAppDeploymentSchedule(),
                new FixCEDeviceID(),
                new ForceLogOff(),
                new ForceReboot(),
                new GetLoggedOnUser(),
                new GetPHPrintFile(),
                new SccmHardwareInventoryCycle(),
                new SccmMachinePolicyRetreivalCycle(),
                new PingTest(),
                new InstallTightVNC(),
                new RemoveTightVNC(),
                new RunGpUpdate(),
                new RunRemoteCommand(),
                new SccmSoftwareInventoryCycle(),
                new UninstallXceleraMonitor()
            };

            _viewModels = new ObservableCollection<ViewModelBase>();
            _viewModels.Add(new ResultConsoleViewModel());

            ProgressData = new ProgressData();
            RunButtonText = "Run";
            Domain = Environment.UserDomainName;
            ActionRunning = false;

            ActionStart += UpdateActionIcon;
            ProgressData.SetTotal += SetProgressTotal;
            ProgressData.SetChange += SetProgressChange;
        }

        protected override void OnDispose()
        {
            ActionsList.Clear();
            ViewModels.Clear(); 
        }

        public void RunCommandExecute()
        {
            if (SelectedAction != null && RunCommandCanExecute())
            {
                OnActionStarted(true);

                ThreadPool.QueueUserWorkItem(
                    o =>
                    {
                        SelectedAction.RunCommand(DeviceListString);
                        OnActionStarted(false);
                    });   
            }
        }

        public bool RunCommandCanExecute()
        {
            if (_actionRunning)
            {
                return false;
            }

            return true;
        }

        private void UpdateActionIcon(bool justStarted)
        {
            ActionRunning = justStarted;

            if (justStarted)
            {
                RunButtonText = "Working";
                return;
            }

            RunButtonText = "Run";
        }

        private void SetProgressTotal(int total)
        {
            ProgressTotal = total;
        }

        private void SetProgressChange(int change)
        {
            ProgressCurrent += change;
        }
    }
}