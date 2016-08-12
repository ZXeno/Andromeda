using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Andromeda.View;
using AndromedaCore;
using AndromedaCore.Infrastructure;
using AndromedaCore.Managers;
using AndromedaCore.ViewModel;

namespace Andromeda.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
#region Properties
        public ObservableCollection<IAction> ActionsList { get; private set; }
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

        private IAction _selectedAction;
        public IAction SelectedAction
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

        private bool _runInParallelWindow = true;
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

        public string VersionNumber => App.VersionNumber;
        public Visibility LoginButtonVisibility => (!CredentialManager.Instance.CredentialsAreValid) ? Visibility.Visible : Visibility.Collapsed;

        private readonly ILoggerService _logger;
        private readonly ActionManager _actionManager;
        #endregion



        #region Constructor
        public MainWindowViewModel(ILoggerService logger, ActionManager actionManager)
        {
            _logger = logger;
            _actionManager = actionManager;
            _viewModels = new ObservableCollection<ViewModelBase> { new ResultConsoleViewModel() };

            RunButtonText = "Run";
            ActionRunning = false;

            ActionManager.ActionStart += UpdateActionIcon;
        }
#endregion

        public void LoadActionsCollection()
        {
            ActionsList = _actionManager.GetObservableActionCollection();
        }

        public void RunCommandExecute()
        {
            if (SelectedAction == null && !RunCommandCanExecute()) { return; }

            if (RunInParallelWindow)
            {
                var dataContext = new ParallelActionWindowViewModel(App.IoC.Resolve<ILoggerService>(), SelectedAction, DeviceListString);
                var newWindow = new ParallelActionWindow
                {
                    DataContext = dataContext
                };
                
                dataContext.Begin();

                newWindow.Show();
                return;
            }

            _actionManager.RunAction(DeviceListString, SelectedAction);
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

        private void UpdateActionIcon(bool justStarted, string actionName)
        {
            ActionRunning = justStarted;

            if (justStarted)
            {
                RunButtonText = "Working";
                return;
            }

            RunButtonText = "Run";
        }

        protected override void OnDispose()
        {
            ActionsList.Clear();
            ViewModels.Clear();
        }
    }
}