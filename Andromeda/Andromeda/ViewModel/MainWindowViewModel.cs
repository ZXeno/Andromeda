using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Andromeda_Actions_Core;
using Andromeda_Actions_Core.Infrastructure;
using Andromeda.View;
using Andromeda_Actions_Core.ViewModel;
using Action = Andromeda_Actions_Core.Action;


namespace Andromeda.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        public delegate void ActionsStarted(bool justStarted);
        public static event ActionsStarted ActionStart;
        public void OnActionStarted(bool justStarted)
        {
            ActionStart?.Invoke(justStarted);
        }


#region Properties
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

        public bool CredentialsValid => CredentialManager.Instance.CredentialsAreValid;

        public string Username
        {
            get
            {
                if (CredentialManager.Instance.CredentialsAreValid)
                {
                    return (CredentialManager.Instance.UserCredentials.Domain + "\\" + CredentialManager.Instance.UserCredentials.User).ToUpper();
                }
                return @"NO USER Please log in";
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

        private bool _runInParallelWindow;
        public bool RunInParallelWindow
        {
            get { return _runInParallelWindow; }
            set
            {
                _runInParallelWindow = value;
                OnPropertyChanged("RunInParallelWindow");
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

        public string VersionNumber => Program.VersionNumber;
        public Visibility UpdateNotification => (Program.UpdateAvailable) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LoginButtonVisibility => (!CredentialManager.Instance.CredentialsAreValid) ? Visibility.Visible : Visibility.Collapsed;
        #endregion



        #region Constructor
        public MainWindowViewModel()
        {
            ActionsList = new ObservableCollection<Action>();
            var actionImportList = new List<Action>();

            // Dynamically get all of our action classes and load them into the viewmodel.
            string @corenamespace = "Andromeda_Actions_Core.Command";
            var assembly = Assembly.LoadFile(Program.WorkingPath + "\\Andromeda-Actions-Core.dll");
            var q = from t in assembly.GetTypes()
                    where t.IsClass && t.Namespace == @corenamespace
                    select t;
            
            foreach (var type in q)
            {
                var assemblyName = assembly.GetName().Name;
                var newinstance = Activator.CreateInstance(assemblyName, type.FullName).Unwrap();
                Action action = newinstance as Action;
                actionImportList.Add(action);
            }

            actionImportList = actionImportList.OrderBy(x => x.ActionName).ToList();

            foreach (var action in actionImportList)
            {
                ActionsList.Add(action);
            }


            _viewModels = new ObservableCollection<ViewModelBase>();
            _viewModels.Add(new ResultConsoleViewModel());

            RunButtonText = "Run";
            ActionRunning = false;

            ActionStart += UpdateActionIcon;
        }
#endregion

        protected override void OnDispose()
        {
            ActionsList.Clear();
            ViewModels.Clear(); 
        }

        public void RunCommandExecute()
        {
            if (SelectedAction == null && !RunCommandCanExecute()) { return; }

            if (RunInParallelWindow)
            {
                var dataContext = new ParallelActionWindowViewModel(SelectedAction, DeviceListString);
                var newWindow = new ParallelActionWindow
                {
                    DataContext = dataContext
                };
                
                dataContext.Begin();
                

                newWindow.Show();
                return;
            }

            OnActionStarted(true);

            var thread = new Thread(
                new ThreadStart(
                    () =>
                    {
                        var thisAction = SelectedAction;
                        Logger.Log("Starting action " + thisAction.ActionName);
                        ResultConsole.Instance.AddConsoleLine("Starting action " + thisAction.ActionName);
                        thisAction.RunCommand(DeviceListString);
                        ResultConsole.Instance.AddConsoleLine("Action " + thisAction.ActionName + " completed.");
                        OnActionStarted(false);
                    }));
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;

            thread.Start();
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
            if (!CredentialManager.Instance.CredentialsAreValid)
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