using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Andromeda_Actions_Core;
using Andromeda_Actions_Core.Infrastructure;
using Action = Andromeda_Actions_Core.Action;

namespace Andromeda
{
    public class Program
    {
        public const string VersionNumber = "Version 0.6";

        public static string WorkingPath = Environment.CurrentDirectory;
        public static string UserFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Andromeda";
        public static string PluginFolder = UserFolder + "\\Plugins";

        private readonly Logger _logger;
        private readonly CredentialManager _credman;
        private readonly ResultConsole _resultConsole;

        public static ConfigManager ConfigManager { get; private set; }
        private static ICollection<Action> _pluginActions;

        public Program()
        {
            if (!Directory.Exists(UserFolder))
            {
                Directory.CreateDirectory(UserFolder);
            }

            if (!Directory.Exists(PluginFolder))
            {
                Directory.CreateDirectory(PluginFolder);
            }

            _logger = new Logger(UserFolder);
            _credman = new CredentialManager();
            ConfigManager = new ConfigManager(UserFolder);
            _resultConsole = ResultConsole.Instance;
        }

        public void Initialize()
        {
            _pluginActions = LoadPlugins();
        }

        public ObservableCollection<Action> LoadActions()
        {
            var actionsList = new ObservableCollection<Action>();
            var actionImportList = new List<Action>();

            // Dynamically get all of our action classes and load them into the viewmodel.
            string @corenamespace = "Andromeda_Actions_Core.Command";
            var assembly = Assembly.LoadFile(WorkingPath + "\\Andromeda-Actions-Core.dll");
            var q = from t in assembly.GetTypes()
                    where t.IsClass && t.Namespace == @corenamespace
                    select t;

            foreach (var type in q)
            {
                var assemblyName = assembly.GetName().Name;
                var newinstance = Activator.CreateInstance(assemblyName, type.FullName).Unwrap();
                var action = newinstance as Action;
                if (action != null)
                {
                    actionImportList.Add(action);
                }
            }

            actionImportList = actionImportList.OrderBy(x => x.ActionName).ToList();

            foreach (var action in actionImportList)
            {
                actionsList.Add(action);
            }

            return actionsList;
        }

        private ICollection<Action> LoadPlugins()
        {
            Logger.Log($"Loading plugins from folder path: {PluginFolder}");

            string[] dllFileNames = null;

            if (!Directory.Exists(PluginFolder))
            {
                Logger.Log("Unable to find plugins directory. Creating...");
                Directory.CreateDirectory(PluginFolder);

                return new List<Action>();
            }

            dllFileNames = Directory.GetFiles(PluginFolder, "*.dll");

            var assemblies = new List<Assembly>(dllFileNames.Length);
            foreach (var dllFile in dllFileNames)
            {
                var an = AssemblyName.GetAssemblyName(dllFile);
                var assembly = Assembly.Load(an);
                assemblies.Add(assembly);
            }

            var pluginType = typeof (Action);
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

            var plugins = new List<Action>(pluginTypes.Count);
            foreach (var type in pluginTypes)
            {
                var plugin = (Action) Activator.CreateInstance(type);
                plugins.Add(plugin);
            }

            return plugins;
        }
    }
}