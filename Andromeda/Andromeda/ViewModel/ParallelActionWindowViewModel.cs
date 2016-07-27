using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using AndromedaCore.Managers;
using AndromedaCore.Infrastructure;
using AndromedaCore.ViewModel;
using Action = AndromedaCore.Action;

namespace Andromeda.ViewModel
{
    public class ParallelActionWindowViewModel : ViewModelBase, IRequestCloseViewModel
    {
        public event EventHandler RequestClose;
        private void OnRequestClose(EventArgs e)
        {
            RequestClose?.Invoke(this, e);
        }

    #region Properties
        public string ViewModelGuid { get; }

        private string _windowMessage;
        public string WindowMessage
        {
            get { return _windowMessage; }
            set
            {
                _windowMessage = value;
                OnPropertyChanged("WindowMessage");
            }
        }

        private bool _cancelRunningAction;
        public bool CancelRunningAction
        {
            get { return _cancelRunningAction; }
            set
            {
                _cancelRunningAction = value;
                OnPropertyChanged("CancelRunningAction");
            }
        }

        private ICommand _cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    CancelCommand = new DelegateCommand(param => CancelCommandExecute(this, null), param => CancelCommandCanExecute());
                }
                return _cancelCommand;
            }
            set
            {
                _cancelCommand = value;
                OnPropertyChanged("CancelCommand");
            }
        }

        private bool _actionCompleted;
        public bool ActionCompleted
        {
            get { return _actionCompleted; }
            set
            {
                _actionCompleted = value;
                OnPropertyChanged("ActionCompleted");
            }
        }
    #endregion

        private readonly Thread _actionThread;
        private readonly Action _runningAction;
        public string ActionTitle => _runningAction.ActionName;
        private readonly ILoggerService _logger;

        public ParallelActionWindowViewModel(ILoggerService logger, Action action, string deviceList)
        {
            _logger = logger;
            ViewModelGuid = Guid.NewGuid().ToString();

            _runningAction = action;
            WindowMessage = "Running action " + _runningAction.ActionName + ". \n Please wait...";
            _runningAction.CancellationRequest += CancelCommandExecute;

            _actionThread = new Thread(
                    new ThreadStart(
                        () =>
                        {
                            _logger.LogMessage("Starting action " + _runningAction.ActionName);
                            ActionManager.OnActionStarted(true, _runningAction.ActionName);
                            ResultConsole.Instance.AddConsoleLine("Starting action " + _runningAction.ActionName);
                            _runningAction.RunCommand(deviceList);
                            if (!_runningAction.CancellationToken.IsCancellationRequested)
                            {
                                var msg = $"Action {_runningAction.ActionName} completed.";
                                _logger.LogMessage(msg);
                                ResultConsole.Instance.AddConsoleLine(msg);
                                ActionManager.OnActionStarted(false, _runningAction.ActionName);
                            }
                            else
                            {
                                var msg = $"Action {_runningAction.ActionName} canceled.";
                                _logger.LogMessage(msg);
                                ResultConsole.Instance.AddConsoleLine(msg);
                                ActionManager.OnActionStarted(false, _runningAction.ActionName);
                            }
                            
                            ActionCompleted = true;
                        }));

            _actionThread.SetApartmentState(ApartmentState.STA);
            _actionThread.IsBackground = false;
            
            PropertyChanged += ProcessActionComplete;
            Application.Current.Exit += OnApplicationExit;
        }

        public void Begin()
        {
            _actionThread.Start();
        }

        private void ProcessActionComplete(object sender, EventArgs e)
        {
            var args = (PropertyChangedEventArgs)e;
            if (args.PropertyName != "ActionCompleted" && !ActionCompleted) { return; }
            
            PropertyChanged -= ProcessActionComplete;
            _actionThread.Join();
            OnRequestClose(null);
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            CancelCommandExecute(this, e);
            PropertyChanged -= ProcessActionComplete;
            _actionThread.Join();
            OnRequestClose(null);
        }

        public void CancelCommandExecute(object sender, EventArgs e)
        {
            CancelRunningAction = true;
            lock (_runningAction)
            {
                _runningAction.CancellationToken.Cancel();
            }
        }

        public bool CancelCommandCanExecute()
        {
            return _runningAction != null;
        }
    }
}