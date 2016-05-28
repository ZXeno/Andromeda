using System;
using System.Collections.ObjectModel;
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
        public bool CredentialsValid => CredentialManager.Instance.CredentialsAreValid;

        private ObservableCollection<ViewModelBase> _viewModels;
        public ObservableCollection<ViewModelBase> ViewModels => _viewModels ?? (_viewModels = new ObservableCollection<ViewModelBase>());

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
                RunCommand = new DelegateCommand(param => RunCommandExecute(), param => RunCommandCanExecute());
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
        public Visibility LoginButtonVisibility => (!CredentialManager.Instance.CredentialsAreValid) ? Visibility.Visible : Visibility.Collapsed;
        #endregion



        #region Constructor
        public MainWindowViewModel()
        {
            _viewModels = new ObservableCollection<ViewModelBase> { new ResultConsoleViewModel() };

            RunButtonText = "Run";
            ActionRunning = false;

            ActionStart += UpdateActionIcon;
        }
#endregion

        public void LoadActionsCollection(ObservableCollection<Action> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection),"Actions collection cannot be null");
            }

            ActionsList = new ObservableCollection<Action>(collection);
        }

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
            return !_actionRunning && !string.IsNullOrWhiteSpace(DeviceListString);
        }

        public bool LoginCommandCanExecute()
        {
            return !CredentialManager.Instance.CredentialsAreValid;
        }

        public void LoginCommandExecute()
        {
            var loginWindow = new LoginWindow();
            var loginWindowViewModel = new LoginWindowViewModel
            {
                SuccessAction = () => loginWindow.DialogResult = true,
                CancelAction = () => loginWindow.DialogResult = false
            };
            loginWindow.DataContext = loginWindowViewModel;

            // Show login prompt
            loginWindow.ShowDialog();

            if (loginWindowViewModel.WasCanceled) { return; }

            UpdateLoginProperties();
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