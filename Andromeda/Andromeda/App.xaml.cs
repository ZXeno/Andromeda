using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.IO;
using System.Windows;
using System.Xml;

namespace Andromeda
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string WorkingDirectoryPath = Environment.CurrentDirectory;
        public const string ConfigFileName = "config.dat";
        public const string CommandsFileName = "commands.xml";
        public const string CommandsDirectory = "\\commands\\";
        public const string ResultsDirectory = "\\results\\";
        public XmlDocument ConfigFile;
        public XmlDocument CommandsFile;

        public void ImportConfiguration()
        {
            if (CheckForConfigFile())
            {
                ConfigFile = XMLImport.GetXMLFileData(WorkingDirectoryPath + "\\" + ConfigFileName);
            }
            else
            {

            }
        }

        public void ImportCommands()
        {
            CommandImport.ImportCommands();
        }

        private bool CheckForConfigFile()
        {
            return File.Exists(Environment.CurrentDirectory + "\\" + "config.ini");
        }

        private void CreateConfigFile()
        {

        }
    }
}
