using System;
using System.ComponentModel;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Windows.Input;
using Andromeda_Actions_Core;
using Andromeda_Actions_Core.Infrastructure;
using Andromeda_Actions_Core.ViewModel;
using Action = Andromeda_Actions_Core.Action;

namespace Andromeda.ViewModel
{
    public class ParallelActionWindowViewModel : ViewModelBase, IRequestCloseViewModel
    {
        public event EventHandler RequestClose;
        private void OnRequestClose(EventArgs e)
        {
            RequestClose?.Invoke(this, e);
        }

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

        private readonly Thread _actionThread;
        private readonly Action _runningAction;
        public string ActionTitle => _runningAction.ActionName;

        public ParallelActionWindowViewModel(Action action, string deviceList)
        {
            ViewModelGuid = Guid.NewGuid().ToString();

            _runningAction = action;
            WindowMessage = "Running action " + _runningAction.ActionName + ". \n Please wait...";
            _runningAction.CancellationRequest += CancelCommandExecute;

            _actionThread = new Thread(
                    new ThreadStart(
                        () =>
                        {
                            Logger.Log("Starting action " + _runningAction.ActionName);
                            ResultConsole.Instance.AddConsoleLine("Starting action " + _runningAction.ActionName);
                            _runningAction.RunCommand(deviceList);
                            if (!_runningAction.CancellationToken.IsCancellationRequested)
                            {
                                ResultConsole.Instance.AddConsoleLine("Action " + _runningAction.ActionName + " completed.");
                            }
                            else
                            {
                                ResultConsole.Instance.AddConsoleLine("Action " + _runningAction.ActionName + " canceled.");
                            }
                            
                            ActionCompleted = true;
                        }));
            _actionThread.SetApartmentState(ApartmentState.STA);
            _actionThread.IsBackground = false;
            
            PropertyChanged += ProcessActionComplete;
        }

        public void Begin()
        {
            _actionThread.Start();
        }

        private void ProcessActionComplete(object sender, EventArgs e)
        {
            PropertyChangedEventArgs args = (PropertyChangedEventArgs)e;
            if (args.PropertyName != "ActionCompleted" && !ActionCompleted) { return; }
            
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
            if (_runningAction != null)
            {
                return true;
            }

            return false;
        }

        
    }
}