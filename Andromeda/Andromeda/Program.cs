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
        public const string VersionNumber = "Version 0.5";

        public static string WorkingPath = Environment.CurrentDirectory;
        public static string UserFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Andromeda";
        public static string DirectoryToCheckForUpdate = "\\\\PATH\\TO\\ANDROMEDA\\FILES\\";

        private readonly Logger _logger;
        private readonly CredentialManager _credman;
        private readonly ResultConsole _resultConsole;

        public static ConfigManager ConfigManager { get; private set; }
        public static bool UpdateAvailable { get; private set; }

        public Program()
        {
            if (!Directory.Exists(UserFolder))
            {
                Directory.CreateDirectory(UserFolder);
            }

            _logger = new Logger(UserFolder);
            _credman = new CredentialManager();
            ConfigManager = new ConfigManager(UserFolder);
            _resultConsole = ResultConsole.Instance;
        }

        public void Initialize()
        {
            CheckForNewVersion();
        }

        public ObservableCollection<Action> LoadActions()
        {
            var actionsList = new ObservableCollection<Action>();
            var actionImportList = new List<Action>();

            // Dynamically get all of our action classes and load them into the viewmodel.
            string @corenamespace = "Andromeda_Actions_Core.Command";
            var assembly = Assembly.LoadFile(Program.WorkingPath + "\\Andromeda-Actions-Core.dll");
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

        private void CheckForNewVersion()
        {
            if(Directory.Exists(DirectoryToCheckForUpdate))
            {
                if (File.Exists(DirectoryToCheckForUpdate + "andromeda.exe"))
                {
                    var hostedAndromeda = File.GetLastWriteTimeUtc(DirectoryToCheckForUpdate + "andromeda.exe");
                    var localAndromeda = File.GetLastWriteTimeUtc(WorkingPath + "\\andromeda.exe");
                    var result = hostedAndromeda.CompareTo(localAndromeda);
                    if (result > 0)
                    {
                        UpdateAvailable = true;
                    }
                }

                if (File.Exists(DirectoryToCheckForUpdate + "Andromeda-Actions-Core.dll"))
                {
                    if (File.Exists(WorkingPath + "\\Andromeda-Actions-Core.dll"))
                    {
                        var hostedAndromeda =
                            File.GetLastWriteTimeUtc(DirectoryToCheckForUpdate + "Andromeda-Actions-Core.dll");
                        var localAndromeda = File.GetLastWriteTimeUtc(WorkingPath + "\\Andromeda-Actions-Core.dll");
                        var result = hostedAndromeda.CompareTo(localAndromeda);
                        if (result > 0)
                        {
                            UpdateAvailable = true;
                        }
                    }
                    else
                    {
                        UpdateAvailable = true;
                    }
                }
            }
        }
    }
}