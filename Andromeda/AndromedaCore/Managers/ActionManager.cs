using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AndromedaCore.Infrastructure;
using AndromedaCore.Model;

namespace AndromedaCore.Managers
{
    public class ActionManager
    {
        private static ActionManager Instance { get; set; }

        public delegate void LoadedActionsChangedEvent();
        public static event LoadedActionsChangedEvent LoadedActionsChanged;
        public static void OnLoadedActionsChanged()
        {
            LoadedActionsChanged?.Invoke();
        }

        public delegate void ActionsStarted(bool justStarted, string actionName);
        public static event ActionsStarted ActionStart;
        public static void OnActionStarted(bool justStarted, string actionName)
        {
            ActionStart?.Invoke(justStarted, actionName);
        }

        private IAction this[string i] => _loadedActions[i];
        private Dictionary<string, IAction> _loadedActions { get; }
        private readonly ILoggerService _logger;
        
        public ObservableCollection<RunningActionTask> RunningActions { get; }
        private readonly StaTaskScheduler _staTaskScheduler;

        // Constructor
        public ActionManager(ILoggerService logger)
        {
            _loadedActions = new Dictionary<string, IAction>();
            RunningActions = new ObservableCollection<RunningActionTask>();
            _staTaskScheduler = new StaTaskScheduler(Environment.ProcessorCount); 
            _logger = logger;
            Instance = this;
        }

        /// <summary>
        /// Adds action to the LoadedActions dictionary.
        /// </summary>
        /// <param name="instantiatedAction"></param>
        public void AddAction(IAction instantiatedAction)
        {
            if (!_loadedActions.ContainsKey(instantiatedAction.ActionName))
            {
                _loadedActions.Add(instantiatedAction.ActionName, instantiatedAction);
                _logger.LogMessage($"Action {instantiatedAction.ActionName} loaded.");
                OnLoadedActionsChanged();
            }
        }

        /// <summary>
        /// Adds an enumerable list of actions to the loaded actions dictionary.
        /// </summary>
        /// <param name="instantiatedActionsList"></param>
        public void AddActions(IEnumerable<IAction> instantiatedActionsList)
        {
            foreach (var action in instantiatedActionsList)
            {
                if (!_loadedActions.ContainsKey(action.ActionName))
                {
                    AddAction(action);
                }
            }
        }

        /// <summary>
        /// Removes a single action from the LoadedActions dictionary.
        /// </summary>
        /// <param name="actionName"></param>
        public static void RemoveAction(string actionName)
        {
            if (Instance._loadedActions.ContainsKey(actionName))
            {
                Instance._loadedActions.Remove(actionName);
                OnLoadedActionsChanged();
            }
        }

        /// <summary>
        /// Clears the LoadedActions dictionary.
        /// </summary>
        public void UnloadAll()
        {
            _loadedActions.Clear();
            OnLoadedActionsChanged();
        }

        /// <summary>
        /// Returns the list of loaded actions as an ObservableCollection.
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<IAction> GetObservableActionCollection()
        {
            return new ObservableCollection<IAction>(_loadedActions.Values.ToList());
        }

        /// <summary>
        /// Runs an action referenced by ActionName property in its own thread.
        /// </summary>
        /// <param name="deviceListString"></param>
        /// <param name="actionName"></param>
        public void RunAction(string deviceListString, string actionName)
        {
            var actiontype =  _loadedActions[actionName].GetType();
            var action = ActionFactory.InstantiateAction(actiontype);
            RunAction(deviceListString, action);
        }

        /// <summary>
        /// Runs a given action in its own thread.
        /// </summary>
        /// <param name="deviceListString"></param>
        /// <param name="action"></param>
        public void RunAction(string deviceListString, IAction action)
        {
            if (action.HasUserInterfaceElement)
            {
                try
                {
                    action.OpenUserInterfaceElement(deviceListString);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message, e);
                    ResultConsole.Instance.AddConsoleLine($"There was an error running {action.ActionName}. Please see the log.");
                }
            }

            var t = Task.Factory.StartNew(
                () =>
                {
                    OnActionStarted(true, action.ActionName);

                    var message1 = $"Starting action {action.ActionName}. Time: {DateTime.Now:HH:mm:ss}";
                    _logger.LogMessage(message1);
                    ResultConsole.Instance.AddConsoleLine(message1);
                    if (!action.CancellationToken.IsCancellationRequested)
                    {
                        if (action.HasUserInterfaceElement)
                        {
                            try
                            {
                                action.UiCallback.Invoke();
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(
                                    $"Action {action.ActionName} was unable to perform it's action because the callback was null.",
                                    e);
                                ResultConsole.Instance.AddConsoleLine($"Action {action.ActionName} was unable to perform it's action. See the log for error details.");
                            }
                        }
                        else
                        {
                            try
                            {
                                action.RunCommand(deviceListString);
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e.Message, e);
                                ResultConsole.Instance.AddConsoleLine($"There was an error running {action.ActionName}. Please see the log.");
                                action.CancellationToken.Cancel();
                            }
                        }
                    }

                    if (!action.CancellationToken.IsCancellationRequested)
                    {
                        var msg = $"Action {action.ActionName} completed.";
                        _logger.LogMessage(msg);
                        ResultConsole.Instance.AddConsoleLine(msg);
                    }
                    else
                    {
                        var msg = $"Action {action.ActionName} canceled.";
                        _logger.LogMessage(msg);
                        ResultConsole.Instance.AddConsoleLine(msg);
                    }

                    OnActionStarted(false, action.ActionName);
                }, CancellationToken.None, TaskCreationOptions.None, _staTaskScheduler);

            var newRunningAction = new RunningActionTask
            {
                RawDeviceListString = deviceListString,
                RunningActionName = action.ActionName,
                ThisActionsTask = t,
                ThreadId = t.Id,
                RunningAction = action
            };

            RunningActions.Add(newRunningAction);

            t.ContinueWith(x => ThreadEnd(newRunningAction.ThreadId));
        }

        private void ThreadEnd(int threadId)
        {
            RunningActions.Remove(RunningActions.FirstOrDefault(t => t.ThreadId == threadId));
        }

        /// <summary>
        /// Injects a list of actions into the ActionManager. Primarily used by plugins to inject actions.
        /// </summary>
        /// <param name="enumerableActions"></param>
        public static void InjectActions(IEnumerable<IAction> enumerableActions)
        {
            if (enumerableActions != null)
            {
                Instance.AddActions(enumerableActions);
            }
        }
    }
}