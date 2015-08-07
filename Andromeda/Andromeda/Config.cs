using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Windows;
using Andromeda.ViewModel;

namespace Andromeda
{
    public class Config
    {
        private bool _saveOfflineComputers = true;
        private bool _saveOnlineComputers = true;
        private string _resultsDirectory;
        private bool _alwaysDumpConsoleHistory = true;
        private string _componentsDirectory = "\\\\melvin\\Andromeda\\components";
        private const string _failedConnectListFile = "failed_to_connect.txt";
        private const string _successfulConnectionListFile = "connection_succeeded_list.txt";

        public bool SaveOfflineComputers { get { return _saveOfflineComputers; } }
        public bool SaveOnlineComputers { get { return _saveOnlineComputers; } }
        public bool AlwaysDumpConsoleHistory { get { return _alwaysDumpConsoleHistory; } }
        public string ResultsDirectory { get { return _resultsDirectory; } }
        public string ComponentDirectory { get { return _componentsDirectory; } }
        public string FailedConnectListFile { get { return _failedConnectListFile; }}
        public string SuccessfulConnectionListFile { get { return _successfulConnectionListFile; } }

        private XmlWriter _xwriter;
        private XmlDocument configFileDat;

        public Config() { }

        public Config(string FilePath)
        {
            _resultsDirectory = Program.UserFolder + "\\results";
            if (!Directory.Exists(_resultsDirectory))
            {
                Directory.CreateDirectory(_resultsDirectory);
            }

            CreateNewConfigFile(FilePath);
        }

        public Config(XmlDocument configFile)
        {
            try
            {
                configFileDat = configFile;

                // "config" node

                _saveOfflineComputers = StringToBool(configFileDat.SelectSingleNode("config/settings/saveofflinecomputers").InnerText);
                _saveOnlineComputers = StringToBool(configFileDat.SelectSingleNode("config/settings/saveonlinecomputers").InnerText);
                _alwaysDumpConsoleHistory = StringToBool(configFileDat.SelectSingleNode("config/settings/alwaysDumpConsoleHistory").InnerText);
                _resultsDirectory = configFileDat.SelectSingleNode("config/settings/resultsDirectory").InnerText;
                _componentsDirectory = configFileDat.SelectSingleNode("config/settings/componentsDirectory").InnerText;

                ResultConsole.Instance.AddConsoleLine("Configuration file loaded.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Uh oh! \n\n The config file failed to load with error message:\n" + ex.Message);
            }

        }
        #region create new config file
        public void CreateNewConfigFile(string filePath)
        {
            ResultConsole.Instance.AddConsoleLine("Generating new config file...");

            #region Document Creation
            try
            {
                XmlWriterSettings _xsets = new XmlWriterSettings();
                _xsets.Encoding = UTF8Encoding.UTF8;
                _xsets.Indent = true;

                _xwriter = XmlWriter.Create(filePath, _xsets);
                
                _xwriter.WriteStartDocument();
                _xwriter.WriteStartElement("config");

                //Program Settings Category
                _xwriter.WriteStartElement("settings");

                // Save Offline Computers
                CreateUnattributedElement("saveofflinecomputers", _saveOfflineComputers.ToString());

                // Save Online Computers
                CreateUnattributedElement("saveonlinecomputers", _saveOnlineComputers.ToString());

                // Always Dump console history on exit
                CreateUnattributedElement("alwaysDumpConsoleHistory", _alwaysDumpConsoleHistory.ToString());

                // Results Log File Directory
                CreateUnattributedElement("resultsDirectory", _resultsDirectory);

                // Components Directory
                CreateUnattributedElement("componentsDirectory", _componentsDirectory);

                // Close <settings>
                _xwriter.WriteEndElement();

                // Close <config>
                _xwriter.WriteEndElement();

                // Close file
                _xwriter.WriteEndDocument();
                _xwriter.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            #endregion

        }
        #endregion

        public void UpdateConfigDocument(XmlDocument configdat, string path)
        {
            configFileDat = configdat;
            configFileDat.Save(path);
            ResultConsole.Instance.AddConsoleLine("Config file updated.");
        }

        private void CreateSingleAttributeElement(string ElementName, string AttributeString1, string AttributeString2)
        {
            _xwriter.WriteStartElement(ElementName);
            _xwriter.WriteAttributeString(AttributeString1, AttributeString2);
            _xwriter.WriteEndElement();
        }

        private void CreateUnattributedElement(string ElementName, string StringData)
        {
            _xwriter.WriteStartElement(ElementName);
            _xwriter.WriteString(StringData);
            _xwriter.WriteEndElement();
        }

        private bool StringToBool(string tfval)
        {
            if (tfval == "True" || tfval == "true" || tfval == "1" || tfval == "T" || tfval == "t" || tfval == "y" || tfval == "Y")
            {
                return true;
            }

            return false;
        }
    }
}
