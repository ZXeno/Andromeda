using System;
using System.IO;
using Andromeda_Actions_Core;
using Andromeda_Actions_Core.Infrastructure;

namespace Andromeda
{
    public class Program
    {
        public const string VersionNumber = "Version 0.5 --EXPERIMENTAL--";

        public static string WorkingPath = Environment.CurrentDirectory;
        public static string UserFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Andromeda";
        public static string DirectoryToCheckForUpdate = "\\\\melvin\\Andromeda\\";

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

            CheckForNewVersion();
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