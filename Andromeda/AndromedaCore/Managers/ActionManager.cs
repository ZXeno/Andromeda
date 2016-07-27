using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using AndromedaCore.Infrastructure;

namespace AndromedaCore.Managers
{
    public class ActionManager
    {
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

        public Action this[string i] => _loadedActions[i];
        private Dictionary<string, Action> _loadedActions { get; }
        private readonly ILoggerService _logger;

        // Constructor
        public ActionManager(ILoggerService logger)
        {
            _loadedActions = new Dictionary<string, Action>();
            _logger = logger;
        }

        /// <summary>
        /// Adds action to the LoadedActions dictionary.
        /// </summary>
        /// <param name="instantiatedAction"></param>
        public void AddAction(Action instantiatedAction)
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
        public void AddActions(IEnumerable<Action> instantiatedActionsList)
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
        public void RemoveAction(string actionName)
        {
            if (_loadedActions.ContainsKey(actionName))
            {
                _loadedActions.Remove(actionName);
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
        public ObservableCollection<Action> GetObservableActionCollection()
        {
            return new ObservableCollection<Action>(_loadedActions.Values.ToList());
        }

        /// <summary>
        /// Runs a given action in its own thread.
        /// </summary>
        /// <param name="deviceListString"></param>
        /// <param name="action"></param>
        public void RunAction(string deviceListString, Action action)
        {
            var thread = new Thread(
                new ThreadStart(
                    () =>
                    {
                        OnActionStarted(true, action.ActionName);
                        _logger.LogMessage($"Starting action {action.ActionName}");
                        ResultConsole.Instance.AddConsoleLine($"Starting action {action.ActionName}");
                        action.RunCommand(deviceListString);
                        ResultConsole.Instance.AddConsoleLine($"Action {action.ActionName} completed.");
                        OnActionStarted(false, action.ActionName);
                    }));
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;

            thread.Start();
        }
        
        /// <summary>
        /// Runs an action referenced by ActionName property in its own thread.
        /// </summary>
        /// <param name="deviceListString"></param>
        /// <param name="actionName"></param>
        public void RunAction(string deviceListString, string actionName)
        {
            RunAction(deviceListString, _loadedActions[actionName]);
        }
    }
}