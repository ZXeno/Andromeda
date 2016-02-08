using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Andromeda.Infrastructure;
using Andromeda.Model;
using Andromeda.View;
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

        public bool CredentialsValid
        {
            get { return Program.CredentialManager.CredentialsAreValid; }
        }

        public string Username
        {
            get
            {
                if (Program.CredentialManager.CredentialsAreValid)
                {
                    return (Program.CredentialManager.UserCredentials.Domain + "\\" + Program.CredentialManager.UserCredentials.User).ToUpper();
                }
                return @"NO USER Please log in";
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

        private ICommand _loginCommand;
        public ICommand LoginCommand
        {
            get
            {
                if (_loginCommand == null)
                {
                    LoginCommand = new DelegateCommand(param => LoginCommandExecute(), param => LoginCommandCanExecute());
                }
                return _loginCommand;
            }
            set
            {
                _loginCommand = value;
                OnPropertyChanged("LoginCommand");
            }
        }

        public Visibility LoginButtonVisibility
        {
            get { return (!Program.CredentialManager.CredentialsAreValid) ? Visibility.Visible : Visibility.Collapsed; }
        }

        public MainWindowViewModel()
        {
            ActionsList = new ObservableCollection<Action>();

            // Dynamically get all of our action classes and load them into the viewmodel.
            string @namespace = "Andromeda.Logic.Command";

            var q = from t in Assembly.GetExecutingAssembly().GetTypes()
                    where t.IsClass && t.Namespace == @namespace
                    select t;
            q = q.OrderBy(x => x.Name).ToList();

            foreach (var type in q)
            {
                var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                var newinstance = Activator.CreateInstance(assemblyName, type.FullName).Unwrap();
                Action action = newinstance as Action;
                ActionsList.Add(action);
                
            }
            // That's pretty fuckin' rad, right? I think so.


            _viewModels = new ObservableCollection<ViewModelBase>();
            _viewModels.Add(new ResultConsoleViewModel());

            RunButtonText = "Run";
            ActionRunning = false;

            ActionStart += UpdateActionIcon;
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

                var thread = new Thread(
                    new ThreadStart(
                        () => {
                                Logger.Log("Starting action " + SelectedAction.ActionName);
                                ResultConsole.Instance.AddConsoleLine("Starting action " + SelectedAction.ActionName);
                                SelectedAction.RunCommand(DeviceListString);
                                OnActionStarted(false);
                            }));
                thread.SetApartmentState(ApartmentState.STA);
                thread.IsBackground = true;

                thread.Start();

                //ThreadPool.QueueUserWorkItem(
                //    o => 
                //    {
                //        Logger.Log("Starting action " + SelectedAction.ActionName);
                //        SelectedAction.RunCommand(DeviceListString);
                //        OnActionStarted(false);
                //        ProgressData.Reset();
                //    }
                //    , ApartmentState.STA);   
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

        public bool LoginCommandCanExecute()
        {
            if (!Program.CredentialManager.CredentialsAreValid)
            {
                return true;
            }

            return false;
        }

        public void LoginCommandExecute()
        {
            LoginWindow loginWindow = new LoginWindow();
            var loginWindowViewModel = new LoginWindowViewModel();
            loginWindowViewModel.SuccessAction = new System.Action(() => loginWindow.DialogResult = true);
            loginWindowViewModel.CancelAction = new System.Action(() => loginWindow.DialogResult = false);
            loginWindow.DataContext = loginWindowViewModel;

            // Show login prompt
            loginWindow.ShowDialog();

            if (loginWindowViewModel.WasCanceled)
            {
                // program is closing if the window was canceled.
                return;
            }

            loginWindow = null;
            OnPropertyChanged("Username");
            OnPropertyChanged("LoginButtonVisibility");
        }

        public void UpdateLoginProperties()
        {
            OnPropertyChanged("Username");
            OnPropertyChanged("LoginButtonVisibility");
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
    }
}