using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows;
using Andromeda.Command;
using Andromeda.Credentials;
using Andromeda.MVVM;

namespace Andromeda
{
    public class Program: ObservableObject
    {
        public static string WorkingPath = Environment.CurrentDirectory;
        public const string ConfigFileName = "config.dat";
        public const string CommandsFileName = "commands.xml";
        public const string CommandsDir = "\\commands\\";
        public const string ResultsDir = "\\results\\";
        public XmlDocument ConfigFile;
        //public XmlDocument CommandsFile;

        private CredentialManager _credman;
        private Commands _cmndr;
        private Config _cnfg;
        private string _consoleContent;

        public CredentialManager CredentialManager 
        {
            get { return _credman; }
            set
            {
                _credman = value;
                RaisePropertyChangedEvent("CredentialManager");
            }
        }
        public Config Configuration
        {
            get { return _cnfg; }
            set 
            {
                _cnfg = value;
                RaisePropertyChangedEvent("Configuration");
            }
        }
        public Commands Commander 
        { 
            get { return _cmndr; }
            set
            {
                _cmndr = value;
                RaisePropertyChangedEvent("Commander");
            }
        }
        public string ConsoleContent
        {
            get { return _consoleContent; }
            set
            {
                _consoleContent = value;
                RaisePropertyChangedEvent("ConsoleContent");
            }
        }
        public ObservableCollection<Andromeda.Command.Action> ActionsList
        {
            get 
            { 
                return Commander.ActionsList; 
            }
        }
        

        public Program()
        {
            InitializeConsole();
            ImportConfiguration();
            ImportCommands();
            GatherCredentials();

        }

        public void GatherCredentials()
        {
            _credman = new CredentialManager();
        }

        public void ImportConfiguration()
        {
            string p = WorkingPath + "\\" + ConfigFileName;
            if (CheckForConfigFile())
            {
                try
                {
                    ConfigFile = XMLImport.GetXMLFileData(p);
                    if (ConfigFile != null)
                    {
                        ResultConsole.AddConsoleLine("Configuration file found.");
                        Configuration = new Config(ConfigFile);
                    }
                    else
                    {
                        Configuration = new Config(p);
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
                ResultConsole.AddConsoleLine("No config file found!");
                CreateConfigFile(p);
            }
        }

        public void ImportCommands()
        {
            Commander = new Commands();
        }

        private void InitializeConsole()
        {
            ResultConsole.InitializeResultConsole();
            ResultConsole.ConsoleChange += OnUpdateConsole;
        }

        private void OnUpdateConsole()
        {
            ConsoleContent = ResultConsole.ConsoleString;
        }

        private bool CheckForConfigFile()
        {
            return File.Exists(WorkingPath + "\\" + ConfigFileName);
        }

        private void CreateConfigFile(string pathToFile)
        {
            _cnfg = new Config(WorkingPath + "\\" + ConfigFileName);
            if (CheckForConfigFile())
            {
                ResultConsole.AddConsoleLine("Config file created.");
            }
            else
            {
                ResultConsole.AddConsoleLine("For some reason, the config file location either isn't readable, \n or there was another problem generating the configuration file.");
                ResultConsole.AddConsoleLine("For now, I'm not sure what to do with this, so we'll use a default configuration for the time being.");
            }
        }

        public void InitializeBackEnd()
        {
            ResultConsole.InitializeResultConsole();

        }
    }
}
