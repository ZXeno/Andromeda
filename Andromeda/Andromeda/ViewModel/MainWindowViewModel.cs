using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Instrumentation;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Andromeda.Logic.Command;
using Andromeda.Model;
using Andromeda.MVVM;
using Action = Andromeda.Logic.Action;

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

        public string VersionNumber
        {
            get { return Program.VersionNumber; }
        }

        public Visibility UpdateNotification
        {
            get { return (Program.UpdateAvailable) ? Visibility.Visible : Visibility.Collapsed; }
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

        
        public string Password
        {
            get { return Program.CredentialManager.UserCredentials.GetInsecurePasswordString(); }
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
            ActionsList = new ObservableCollection<Action>();

            // Dynamically get all of our action classes and load them into the viewmodel.
            string @namespace = "Andromeda.Logic.Command";

            var q = from t in Assembly.GetExecutingAssembly().GetTypes()
                    where t.IsClass && t.Namespace == @namespace
                    select t;
            q.ToList().ForEach(t => Console.WriteLine(t.Name));

            foreach (var type in q)
            {
                var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                var newinstance = Activator.CreateInstance(assemblyName, type.FullName).Unwrap();
                Action action = newinstance as Action;
                ActionsList.Add(action);
            }// That's pretty fuckin' rad, right? I think so.


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
                ProgressData.Reset();
                OnActionStarted(true);

                ThreadPool.QueueUserWorkItem(
                    o =>
                    {
                        Logger.Log("Starting action " + SelectedAction.ActionName);
                        SelectedAction.RunCommand(DeviceListString);
                        OnActionStarted(false);
                        ProgressData.Reset();
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
            Logger.Log("Updating action running state to " + justStarted);
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