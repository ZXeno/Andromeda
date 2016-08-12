using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using AndromedaCore.Infrastructure;

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

        public IAction this[string i] => _loadedActions[i];
        private Dictionary<string, IAction> _loadedActions { get; }
        private readonly ILoggerService _logger;

        // Constructor
        public ActionManager(ILoggerService logger)
        {
            _loadedActions = new Dictionary<string, IAction>();
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
        /// Runs a given action in its own thread.
        /// </summary>
        /// <param name="deviceListString"></param>
        /// <param name="action"></param>
        public void RunAction(string deviceListString, IAction action)
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