using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Andromeda_Actions_Core;
using Andromeda_Actions_Core.Infrastructure;
using Andromeda_Actions_Core.Plugins;
using Action = Andromeda_Actions_Core.Action;

namespace Andromeda
{
    public class Program
    {
        public const string VersionNumber = "Version 0.6.1";

        public static string WorkingPath = Environment.CurrentDirectory;
        public static string UserFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Andromeda";
        public static string PluginFolder = WorkingPath + "\\Plugins";
        public static IoCContainer IoC { get; private set; }

        private ILoggerService _logger;
        private CredentialManager _credman;
        private ResultConsole _resultConsole;

        public static ConfigManager ConfigManager { get; private set; }
        private readonly List<IPlugin> _loadedPlugins; 
        private readonly List<Action> _pluginActions;
        private readonly List<Action> _coreActions; 

        internal Program()
        {
            if (!Directory.Exists(UserFolder))
            {
                Directory.CreateDirectory(UserFolder);
            }

            if (!Directory.Exists(PluginFolder))
            {
                Directory.CreateDirectory(PluginFolder);
            }

            _loadedPlugins = new List<IPlugin>();
            _coreActions = new List<Action>();
            _pluginActions = new List<Action>();
        }

        public void Initialize()
        {
            InitializeIoCContainer();

            _logger = IoC.Resolve<ILoggerService>();
            _credman = new CredentialManager();
            _resultConsole = new ResultConsole();
            ConfigManager = new ConfigManager(WorkingPath, IoC.Resolve<IXmlServices>(), IoC.Resolve<ILoggerService>());

            LoadPlugins();
            LoadCoreActions();
            LoadPluginActions();
        }

        public ObservableCollection<Action> RetreiveActionsList()
        {
            var completeList = new List<Action>();

            completeList.AddRange(_coreActions);

            if (_pluginActions.Count > 0)
            {
                completeList.AddRange(_pluginActions);
            }

            var actionsList = new ObservableCollection<Action>();
            completeList.OrderBy(x => x.ActionName).ToList().ForEach((action) => { actionsList.Add(action); });

            return actionsList;
        }

        private void InitializeIoCContainer()
        {
            IoC = new IoCContainer();

            IoC.Register<ILoggerService, Logger>(LifeTimeOptions.ContainerControlledLifeTimeOption);
            IoC.Register<IFileAndFolderServices, FileAndFolderServices>();
            IoC.Register<INetworkServices, NetworkServices>();
            IoC.Register<IPsExecServices, PsExecServices>();
            IoC.Register<IWmiServices, WmiServices>();
            IoC.Register<ISccmClientServices, SccmClientServices>();
            IoC.Register<IXmlServices, XmlServices>();
            IoC.Register<IRegistryServices, RegistryServices>();
            
        }

        private void LoadCoreActions()
        {
            _logger.LogMessage("Loading core Andromeda actions...");
            var actionImportList = new List<Action>();

            // Dynamically get all of our core action classes and load them.
            var @corenamespace = "Andromeda_Actions_Core.Command";
            var assembly = Assembly.LoadFile(WorkingPath + "\\Andromeda-Actions-Core.dll");
            var q = from t in assembly.GetTypes() where t.IsClass && t.Namespace == @corenamespace select t;

            foreach (var type in q)
            {
                var action = InstantiateImportedType(type);

                if (action == null) { continue; }

                actionImportList.Add(action);
            }

            foreach (var action in actionImportList)
            {
                _coreActions.Add(action);
                _logger.LogMessage($"{action.ActionName} loaded.");
            }
        }

        private void LoadPluginActions()
        {
            _logger.LogMessage("Loading actions from loaded plugins...");
            foreach (var plugin in _loadedPlugins)
            {
                var q = plugin.ImportActions();

                foreach (var type in q)
                {
                    var action = InstantiateImportedType(type);

                    if (action == null) continue;

                    _pluginActions.Add(action);
                    _logger.LogMessage($"Action {action.ActionName} loaded.");
                }
            }
        }

        private Action InstantiateImportedType(Type type)
        {
            var constructorInfo = type.GetConstructors().First();
            var paramsInfo = constructorInfo.GetParameters().ToList();
            var resolvedParams = new List<object>();

            foreach (var param in paramsInfo)
            {
                var t = param.ParameterType;
                var res = IoC.Resolve(t);
                resolvedParams.Add(res);
            }

            return constructorInfo.Invoke(resolvedParams.ToArray()) as Action;
        }

        private void LoadPlugins()
        {
            _logger.LogMessage($"Loading plugins from folder path: {PluginFolder}");

            string[] dllFileNames = null;

            if (!Directory.Exists(PluginFolder))
            {
                _logger.LogWarning("Unable to find plugins directory. Creating...", null);
                try
                {
                    Directory.CreateDirectory(PluginFolder);
                }
                catch (Exception e)
                {
                    _logger.LogError("Unable to create plugin directory due to exception", e);
                }

                return;
            }

            dllFileNames = Directory.GetFiles(PluginFolder, "*.dll");

            var assemblies = new List<Assembly>(dllFileNames.Length);
            foreach (var dllFile in dllFileNames)
            {
                try
                {
                    var an = AssemblyName.GetAssemblyName(dllFile);
                    _logger.LogMessage($"Found dll {an.Name}");

                    var assembly = Assembly.Load(an);
                    assemblies.Add(assembly);
                }
                catch (Exception e)
                {
                    _logger.LogError($"There was an error loading the dll file {dllFile}", e);
                }
                
            }

            var pluginType = typeof (IPlugin);
            var pluginTypes = new List<Type>();
            foreach (var assembly in assemblies)
            {
                if (assembly == null) { continue; }

                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (type.IsInterface || type.IsAbstract)
                    {
                        continue;
                    }

                    if (type.GetInterface(pluginType.FullName) != null)
                    {
                        pluginTypes.Add(type);
                    }
                }
            }
            
            foreach (var type in pluginTypes)
            {
                var plugin = (IPlugin)Activator.CreateInstance(type);
                _loadedPlugins.Add(plugin);
                _logger.LogMessage($"Loaded plugin {plugin.PluginName}");
            }
        }
    }
}