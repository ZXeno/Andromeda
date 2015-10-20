using System;
using System.IO;

namespace Andromeda
{
    public class Program
    {
        public const string VersionNumber = "Version 0.2";

        public static string WorkingPath = Environment.CurrentDirectory;
        public static string UserFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Andromeda";

        private Logger _logger;

        private static CredentialManager _credman;
        public static CredentialManager CredentialManager
        {
            get { return _credman; }
            set { _credman = value; }
        }

        public static ResultConsole ResultConsole { get; private set; }

        private static ConfigManager _configMan;
        public static ConfigManager ConfigManager { get { return _configMan; } }
        private static bool _updateAvailable;
        public static bool UpdateAvailable { get { return _updateAvailable; } }

        public Program()
        {
            if (!Directory.Exists(UserFolder))
            {
                Directory.CreateDirectory(UserFolder);
            }

            _logger = new Logger();
            _credman = new CredentialManager();
            _configMan = new ConfigManager();
            ResultConsole = ResultConsole.Instance;

            CheckForNewVersion();
        }

        private void CheckForNewVersion()
        {
            if(Directory.Exists("\\\\melvin\\Andromeda\\"))
            {
                if (File.Exists("\\\\melvin\\andromeda\\andromeda.exe"))
                {
                    var hostedAndromeda = File.GetLastWriteTimeUtc("\\\\melvin\\andromeda\\andromeda.exe");
                    var localAndromeda = File.GetLastWriteTimeUtc(WorkingPath + "\\andromeda.exe");
                    var result = hostedAndromeda.CompareTo(localAndromeda);
                    if (result > 0)
                    {
                        _updateAvailable = true;
                    }
                }
            }
        }
    }
}