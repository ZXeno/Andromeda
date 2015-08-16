using System;
using System.IO;

namespace Andromeda
{
    public class Program
    {
        public const string VersionNumber = "Version 0.1";

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

        public Program()
        {
            if (!Directory.Exists(UserFolder))
            {
                Directory.CreateDirectory(UserFolder);
            }

            _logger = new Logger();
            _credman = new CredentialManager();
            _configMan = new ConfigManager();
            ResultConsole = ResultConsole.Instance;;
        }
    }
}