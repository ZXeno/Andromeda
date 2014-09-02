using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.IO;

namespace Andromeda
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string WorkingDirectoryPath = Environment.CurrentDirectory;
        public const string ConfigFileName = "config.dat";
        public const string CommandsFileName = "commands.xml";
        public const string CommandsDirectory = "\\commands\\";
        public const string ResultsDirectory = "\\results\\";
        public XmlDocument ConfigFile;
        public XmlDocument CommandsFile;

        private ResultConsole rconsole;

        public ResultConsole ResultConsole
        {
            get { return rconsole; }
        }
        public string ConsoleText
        {
            get { return rconsole.ConsoleString; }
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeConsole();
            ImportConfiguration();
        }

        public void ImportConfiguration()
        {
            string p = WorkingDirectoryPath + "\\" + ConfigFileName;
            rconsole.AddConsoleLine("config file path: " + p);
            if (CheckForConfigFile())
            {
                ResultConsole.AddConsoleLine("Configuration file found at: " + p);
                ConfigFile = XMLImport.GetXMLFileData(p);
                if (ConfigFile != null)
                {
                    ResultConsole.AddConsoleLine("Config file loaded. - " + p);
                }
            }
            else
            {
                ResultConsole.AddConsoleLine("No config file found!");
                ResultConsole.AddConsoleLine("Generating new config file...");
                File.CreateText(p);
                CreateConfigFile(p);
            }
        }

        public void ImportCommands()
        {
            CommandImport.ImportCommands();
        }

        public void ExternallyAddToConsole(string s)
        {
            rconsole.AddConsoleLine(s);
        }

        private bool CheckForConfigFile()
        {
            return File.Exists(WorkingDirectoryPath + "\\" + ConfigFileName);
        }

        private void CreateConfigFile(string pathToFile)
        {
            rconsole.AddConsoleLine("I'm pretending to generate a new config file!");
        }

        private void OnUpdateConsole()
        {
            RESULTS_BOX.Text = ResultConsole.ConsoleString;
        }

        private void InitializeConsole()
        {
            rconsole = new ResultConsole();
            ResultConsole.ConsoleChange += OnUpdateConsole;
        }
    }
}
