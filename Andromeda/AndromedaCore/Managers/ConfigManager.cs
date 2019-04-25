using System;
using System.IO;
using AndromedaCore.Infrastructure;
using AndromedaCore.Model;
using Newtonsoft.Json;

namespace AndromedaCore.Managers
{
    public class ConfigManager
    {
        private readonly ILoggerService _logger;

        public static Configuration CurrentConfig { get; set; }

        // DEFAULT CONFIGURATION VALUES
        public const string ConfigFileName = "config.dat";
        private const string SaveFileVersion = "005";
        private bool _enableDeviceCountWarning = true;
        private int _deviceCountWarningThreshold = 25;
        private bool _saveOfflineComputers = true;
        private bool _saveOnlineComputers = true;
        private readonly string _resultsDirectory;
        private readonly string _configFilePath;
        private readonly string _componentsDirectory;
        private const string _failedConnectListFile = "failed_to_connect.txt";
        private const string _successfulConnectionListFile = "connection_succeeded_list.txt";

        public ConfigManager(string path, ILoggerService logger)
        {
            _logger = logger;
            _configFilePath = path + "\\" + ConfigFileName;
            _resultsDirectory = path + "\\Results";
            _componentsDirectory = Environment.CurrentDirectory + "\\Components";

            if (CheckForConfigFile())
            {
                LoadConfig();
                return;
            }

            CreateNewConfigFile();
        }

        private bool ValidateConfigFileVersion(string saveFileVersion)
        {
            if (string.IsNullOrEmpty(saveFileVersion))
            {
                return false;
            }

            if (SaveFileVersion != saveFileVersion)
            {
                var msg = "MISMATCHED CONFIG FILE VERSION. A new one will be generated.";
                _logger.LogMessage(msg);
                ResultConsole.Instance.AddConsoleLine(msg);

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
            _logger.LogMessage("Beginning config file load.");

            if (File.Exists(_configFilePath))
            {
                try
                {
                    string configJson = File.ReadAllText(_configFilePath);

                    CurrentConfig = JsonConvert.DeserializeObject<Configuration>(configJson);

                    if (!ValidateConfigFileVersion(CurrentConfig.SaveFileVersion))
                    {
                        return;
                    }

                }
                catch (Exception ex)
                {
                    ResultConsole.Instance.AddConsoleLine($"Configuration file failed to load with exception: {ex.Message}\n\n A new one will be created.");
                    CreateNewConfigFile();
                    return;
                }
                
                ResultConsole.Instance.AddConsoleLine("Configuration file loaded.");
            }
            else
            {
                CreateNewConfigFile();
            }
        }

        #region create new config file

        public void CreateNewConfigFile()
        {
            _logger.LogMessage("Generating new config file...");
            ResultConsole.Instance.AddConsoleLine("Generating new config file...");

            ValidateDirectoryExists(_resultsDirectory);
            ValidateDirectoryExists(_componentsDirectory);

            try
            {
                CurrentConfig = new Configuration()
                {
                    SaveFileVersion = SaveFileVersion,
                    ComponentDirectory = _componentsDirectory,
                    DataFilePath = _configFilePath,
                    DeviceCountWarningThreshold = _deviceCountWarningThreshold,
                    EnableDeviceCountWarning = _enableDeviceCountWarning,
                    FailedConnectListFile = _failedConnectListFile,
                    ResultsDirectory = _resultsDirectory,
                    SaveOfflineComputers = _saveOfflineComputers,
                    SaveOnlineComputers = _saveOnlineComputers,
                    SuccessfulConnectionListFile = _successfulConnectionListFile
                };

                string serializedConfig = JsonConvert.SerializeObject(CurrentConfig);

                File.WriteAllText(_configFilePath, serializedConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to create configuration file.", ex);
                ResultConsole.Instance.AddConsoleLine("Exception in " + ex.TargetSite + ": " + ex.InnerException?.Message + " - Unable to create configuration file.");
                return;
            }

            _logger.LogMessage("Configuration file generation complete.");
        }
        #endregion

        public void UpdateConfigDocument(string path)
        {
            try
            {
                string serializedConfig = JsonConvert.SerializeObject(CurrentConfig);
                File.WriteAllText(_configFilePath, serializedConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to update configuration file.", ex);
                ResultConsole.Instance.AddConsoleLine($"Unable to update configuration file: {ex.Message}");
                return;
            }
            
            ResultConsole.Instance.AddConsoleLine("Config file updated.");
        }

        private void ValidateDirectoryExists(string path)
        {
            _logger.LogMessage("Validating path " + path);

            if (Directory.Exists(path)) {return;}

            _logger.LogWarning("Directory path " + path + " does not exist. Creating...", null);
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                _logger.LogError("There was an error creating directory " + path + "\\.", e);
                ResultConsole.Instance.AddConsoleLine("There was a problem validating a directory during configuration loading. See the log file.");
            }
        }

        private bool CheckForConfigFile()
        {
            if (File.Exists(_configFilePath))
            {
                ResultConsole.Instance.AddConsoleLine("Configuration file found.");
                _logger.LogMessage("Configuration file found.");
                return true;
            }

            _logger.LogMessage("No config file found!");
            ResultConsole.Instance.AddConsoleLine("No config file found!");
            return false;

        }
    }
}