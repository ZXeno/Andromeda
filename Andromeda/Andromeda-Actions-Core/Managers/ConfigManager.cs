using System;
using System.IO;
using System.Text;
using System.Xml;
using Andromeda_Actions_Core.Infrastructure;
using Andromeda_Actions_Core.Model;

namespace Andromeda_Actions_Core
{
    public class ConfigManager
    {
        public static Configuration CurrentConfig { get; set; }

        // DEFAULT CONFIGURATION VALUES
        public const string ConfigFileName = "config.dat";
        private const string SaveFileVersion = "004";
        private bool _saveOfflineComputers = true;
        private bool _saveOnlineComputers = true;
        private readonly string _resultsDirectory;
        private readonly string _configFilePath;
        private string _componentsDirectory;
        private const string _failedConnectListFile = "failed_to_connect.txt";
        private const string _successfulConnectionListFile = "connection_succeeded_list.txt";

        private XmlWriter _xwriter;
        private XmlDocument configFileDat;
        private readonly IXmlServices _xmlServices;

        public ConfigManager(string userFolder, IXmlServices xmlServices)
        {
            _xmlServices = xmlServices;
            _configFilePath = userFolder + "\\" + ConfigFileName;
            _resultsDirectory = userFolder + "\\Results";
            _componentsDirectory = userFolder + "\\components";

            CurrentConfig = new Configuration
            {
                DataFilePath = _configFilePath,
                FailedConnectListFile = _failedConnectListFile,
                SuccessfulConnectionListFile = _successfulConnectionListFile
            };

            if (CheckForConfigFile())
            {
                LoadConfig();
                return;
            }

            CreateNewConfigFile();
        }

        private bool ValidateConfigFileVersion(XmlNode versionNode)
        {
            if (versionNode == null || versionNode.InnerText != SaveFileVersion)
            {
                Logger.Log("MISMATCHED CONFIG FILE VERSION. A new one will be generated.");
                ResultConsole.Instance.AddConsoleLine("MISMATCHED CONFIG FILE VERSION. A new one will be generated.");

                if (File.Exists(_configFilePath))
                {
                    File.Delete(_configFilePath);
                }

                CreateNewConfigFile();
                return false;
            }

            return true;
        }

        public void LoadConfig()
        {
            Logger.Log("Beginning config file load.");
            try
            {
                configFileDat = _xmlServices.GetXmlFileData(_configFilePath);

                // "config" node

                var saveFileVersionNode = configFileDat.SelectSingleNode("config/settings/savefileversion");
                if (!ValidateConfigFileVersion(saveFileVersionNode))
                {
                    return;
                }

                var saveOfflineNode = configFileDat.SelectSingleNode("config/settings/saveofflinecomputers");
                if (saveOfflineNode != null)
                {
                    CurrentConfig.SaveOfflineComputers = StringToBool(saveOfflineNode.InnerText);
                }
                else
                {
                    Logger.Log("Problem loading \"saveofflinecomputers\" node from config file. Using default.");
                    ResultConsole.Instance.AddConsoleLine("Problem loading \"saveofflinecomputers\" node from config file. Using default: TRUE");
                    CurrentConfig.SaveOfflineComputers = true;
                }

                var saveOnlineNode = configFileDat.SelectSingleNode("config/settings/saveonlinecomputers");
                if (saveOnlineNode != null)
                {
                    CurrentConfig.SaveOnlineComputers = StringToBool(saveOnlineNode.InnerText);
                }
                else
                {
                    Logger.Log("Problem loading \"saveonlinecomputers\" node from config file. Using default.");
                    ResultConsole.Instance.AddConsoleLine("Problem loading \"saveonlinecomputers\" node from config file. Using default: TRUE");
                    CurrentConfig.SaveOnlineComputers = true;
                }

                var resultsDirNode = configFileDat.SelectSingleNode("config/settings/resultsDirectory");
                if (resultsDirNode != null)
                {
                    CurrentConfig.ResultsDirectory = resultsDirNode.InnerText;
                }
                else
                {
                    Logger.Log("Problem loading \"resultsDirectory\" node from config file. Using default.");
                    ResultConsole.Instance.AddConsoleLine("Problem loading \"resultsDirectory\" node from config file. Using default: " + _resultsDirectory);
                    CurrentConfig.ResultsDirectory = _resultsDirectory;
                }

                var componentsDirNode = configFileDat.SelectSingleNode("config/settings/componentsDirectory");
                if (componentsDirNode != null)
                {
                    CurrentConfig.ComponentDirectory = componentsDirNode.InnerText;
                }
                else
                {
                    Logger.Log("Problem loading \"componentsDirectory\" node from config file. Using default.");
                    ResultConsole.Instance.AddConsoleLine( "Problem loading \"componentsDirectory\" node from config file. Using default: " + _componentsDirectory);
                    CurrentConfig.ComponentDirectory = _componentsDirectory;
                }

                ValidateDirectoryExists(CurrentConfig.ResultsDirectory);
                ValidateDirectoryExists(CurrentConfig.ComponentDirectory);


                ResultConsole.Instance.AddConsoleLine("Configuration file loaded.");
            }
            catch (Exception ex)
            {
                throw new Exception("Uh oh! \n\n The config file failed to load with error message:\n" + ex.Message);
            }
        }

        #region create new config file

        public void CreateNewConfigFile()
        {
            Logger.Log("Generating new config file...");
            ResultConsole.Instance.AddConsoleLine("Generating new config file...");

            #region Document Creation
            try
            {
                XmlWriterSettings _xsets = new XmlWriterSettings();
                Logger.Log("Configuration file encoding set to UTF8.");
                _xsets.Encoding = Encoding.UTF8;
                _xsets.Indent = true;

                _xwriter = XmlWriter.Create(_configFilePath, _xsets);
                
                _xwriter.WriteStartDocument();
                _xwriter.WriteStartElement("config");

                //Program Settings Category
                _xwriter.WriteStartElement("settings");

                CreateUnattributedElement("savefileversion", SaveFileVersion);

                // Save Offline Computers
                CreateUnattributedElement("saveofflinecomputers", _saveOfflineComputers.ToString());

                // Save Online Computers
                CreateUnattributedElement("saveonlinecomputers", _saveOnlineComputers.ToString());

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

                Logger.Log("Configuration file generation complete.");

                ValidateDirectoryExists(_resultsDirectory);
                ValidateDirectoryExists(_componentsDirectory);
            }
            catch (Exception ex)
            {
                Logger.Log("Exception in " + ex.TargetSite + ": " + ex.InnerException + " - Unable to create configuration file.");
                ResultConsole.Instance.AddConsoleLine("Exception in " + ex.TargetSite + ": " + ex.InnerException + " - Unable to create configuration file.");
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

        private void CreateUnattributedElement(string ElementName, string StringData)
        {
            _xwriter.WriteStartElement(ElementName);
            _xwriter.WriteString(StringData);
            _xwriter.WriteEndElement();
        }

        private bool StringToBool(string tfval)
        {
            tfval = tfval.ToLower();
            if (tfval == "true" || tfval == "1" || tfval == "t" || tfval == "y" || tfval == "yes" || tfval == "affirmative")
            {
                return true;
            }

            return false;
        }

        private void ValidateDirectoryExists(string path)
        {
            Logger.Log("Validating path " + path);

            if (!Directory.Exists(path))
            {
                Logger.Log("Directory path " + path + " does not exist. Creating...");
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception e)
                {
                    Logger.Log("There was an error creating directory " + path + "\\. Exception: " + e.InnerException);
                    ResultConsole.Instance.AddConsoleLine("There was a problem validating a directory during configuration loading. See the log file.");
                }
            }
        }

        private bool CheckForConfigFile()
        {
            if (File.Exists(_configFilePath))
            {
                ResultConsole.Instance.AddConsoleLine("Configuration file found.");
                Logger.Log("Configuration file found.");
                return true;
            }

            Logger.Log("No config file found!");
            ResultConsole.Instance.AddConsoleLine("No config file found!");
            return false;

        }
    }
}