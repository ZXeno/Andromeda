using System;
using System.IO;
using System.Windows;
using System.Xml;
using Andromeda.ViewModel;

namespace Andromeda
{
    public class Program
    {
        private string _consoleContent;

        public static string WorkingPath = Environment.CurrentDirectory;
        public static string UserFolder;
        public const string ConfigFileName = "config.dat";
        public const string ResultsDir = "\\results\\";

        private Logger _logger;

        private static CredentialManager _credman;
        public static CredentialManager CredentialManager
        {
            get { return _credman; }
            set { _credman = value; }
        }

        private static Config _cnfg;
        private static Config _staticconfig;
        public static Config Config { get { return _staticconfig; } }
        public Config Configuration
        {
            get { return _cnfg; }
            set
            {
                _cnfg = value;
                _staticconfig = value;
            }
        }
        public XmlDocument ConfigFile;

        public Program()
        {
            UserFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Andromeda";

            if (!Directory.Exists(UserFolder))
            {
                Directory.CreateDirectory(UserFolder);
            }

            _logger = new Logger();
            _credman = new CredentialManager();

            string p = UserFolder + "\\" + ConfigFileName;
            if (File.Exists(p))
            {
                try
                {
                    ConfigFile = XMLImport.GetXMLFileData(p);
                    if (ConfigFile != null)
                    {
                        ResultConsole.Instance.AddConsoleLine("Configuration file found.");
                        Logger.Log("Configuration file found.");
                        Configuration = new Config(ConfigFile);
                    }
                    else
                    {
                        Configuration = new Config(p);
                        Logger.Log("Created new configuration file at " + p);
                    }
                }
                catch (FileNotFoundException fnf)
                {
                    MessageBox.Show("File unable to load. \n Exception: " + fnf.Message);
                    App.Current.Shutdown();
                }
            }
            else
            {
                Logger.Log("No config file found!");
                ResultConsole.Instance.AddConsoleLine("No config file found!");
                CreateConfigFile(p);
            }
        }

        private void CreateConfigFile(string pathToFile)
        {
            Logger.Log("Creating new config file...");
            Configuration = new Config(pathToFile);

            if (File.Exists(UserFolder + "\\" + ConfigFileName))
            {
                Logger.Log("Config file created.");
                ResultConsole.Instance.AddConsoleLine("Config file created.");
            }
            else
            {
                ResultConsole.Instance.AddConsoleLine("For some reason, the config file location either isn't readable, \n or there was another problem generating the configuration file.");
                ResultConsole.Instance.AddConsoleLine("For now, I'm not sure what to do with this, so we'll use a default configuration for the time being.");
            }
        }
    }
}