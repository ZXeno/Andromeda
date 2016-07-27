using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AndromedaCore.Infrastructure;

namespace AndromedaCore.Plugins
{
    public class PluginManager
    {
        public delegate void LoadedPluginsChangedEvent();
        public static event LoadedPluginsChangedEvent LoadedPluginsChanged;
        public static void OnLoadedPluginsChanged()
        {
            LoadedPluginsChanged?.Invoke();
        }
        
        public IPlugin this[string i] => _loadedPlugins[i];

        private readonly string _pluginFolder = Environment.CurrentDirectory + "\\Plugins";
        private readonly Dictionary<string, IPlugin> _loadedPlugins;
        private readonly ILoggerService _logger;

        public PluginManager(ILoggerService logger)
        {
            _logger = logger;
            _loadedPlugins = new Dictionary<string, IPlugin>();
        }

        public void LoadPlugin()
        {
            throw new NotImplementedException();
        }

        public void LoadAllPlugins()
        {
            _logger.LogMessage($"Loading plugins from folder path: {_pluginFolder}");

            string[] dllFileNames = null;

            if (!Directory.Exists(_pluginFolder))
            {
                _logger.LogWarning("Unable to find plugins directory. Creating...", null);
                try
                {
                    Directory.CreateDirectory(_pluginFolder);
                }
                catch (Exception e)
                {
                    _logger.LogError("Unable to create plugin directory due to exception", e);
                }

                return;
            }

            dllFileNames = Directory.GetFiles(_pluginFolder, "*.dll");

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

            var pluginType = typeof(IPlugin);
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
                _loadedPlugins.Add(plugin.PluginName, plugin);
                _logger.LogMessage($"Loaded plugin {plugin.PluginName}");
            }
        }

        public void InitializePlugin()
        {
            throw new NotImplementedException();
        }

        public void InitializeAllPlugins()
        {
            foreach (var plugin in _loadedPlugins.Values)
            {
                if (plugin.IsCompatible(Assembly.GetExecutingAssembly().GetName().Version.ToString()))
                {
                    plugin.InitializePlugin();
                }
            }
        }

        public void UnloadPlugin()
        {
            throw new NotImplementedException();
        }

        public void UnloadAllPlugins()
        {
            foreach (var plugin in _loadedPlugins.Values)
            {
                plugin.UnloadPlugin();
            }
        }

        public void RemovePlugin(string pluginName)
        {
            if (_loadedPlugins.ContainsKey(pluginName))
            {
                _loadedPlugins.Remove(pluginName);
            }
        }

        public void UninstallPlugin()
        {
            throw new NotImplementedException();
        }
    }
}