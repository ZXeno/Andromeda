using System;
using AndromedaCore.Managers;
using Microsoft.Win32;

namespace AndromedaCore.Infrastructure
{
    public class RegistryServices : IRegistryServices
    {
        private readonly ILoggerService _logger;

        public RegistryServices(ILoggerService logger)
        {
            _logger = logger;
        }

        public bool ValidateKeyExists(string targetDevice, RegistryHive baseKey, string targetSubKey)
        {
            try
            {
                var targetkey = RegistryKey.OpenRemoteBaseKey(baseKey, targetDevice, RegistryView.Registry64).OpenSubKey(targetSubKey);

                if (targetkey != null)
                {
                    _logger.LogMessage("Attempt to validate registry key succeeded.");
                    targetkey.Close();
                    return true;
                }

                _logger.LogWarning("Attempt to validate registry key, but targetkey value returned null. Check the path and try again.", null);
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError($"Unable to validate remote registy key {targetSubKey}: {e.Message}.", e);
                ResultConsole.Instance.AddConsoleLine($"Unable to validate key {targetSubKey} on device {targetDevice}.");
                return false;
            }
        }

        public bool ValidateKeyValueExists(RegistryKey subKey, string targetKeyValue, string expectedName)
        {
            return false;
        }

        public RegistryKey GetRegistryKey(string targetDevice, RegistryHive baseKey, string targetSubKey)
        {
            if (string.IsNullOrEmpty(targetSubKey))
            {
                _logger.LogError($"targetSubKey value must not be null or empty. GetRegistryKey({targetDevice}, {baseKey}, {targetSubKey}) call contains an empty subkey.", null);
                return null;
            }

            try
            {
                return RegistryKey.OpenRemoteBaseKey(baseKey, targetDevice, RegistryView.Registry64).OpenSubKey(targetSubKey, true);
            }
            catch (Exception e)
            {
                _logger.LogError($"Unable to get remote registy key {targetSubKey}: {e.Message}.", e);
                ResultConsole.Instance.AddConsoleLine($"Unable to get remote registry key {targetSubKey} on device {targetDevice}.");
                return null;
            }
        }

        public void DeleteSubkeyTree(string targetDevice, RegistryHive baseKey, string targetSubKey)
        {
            if (string.IsNullOrEmpty(targetSubKey))
            {
                ResultConsole.Instance.AddConsoleLine($"Unable to remove remote registry subkey tree on device {targetDevice}. Subkey provided was empty.");
                _logger.LogWarning($"targetSubKey value must not be null or empty. DeleteSubkeyTree({targetDevice}, {baseKey}, {targetSubKey}) call contains an empty subkey.", null);
                return;
            }

            if (ValidateKeyExists(targetDevice, baseKey, targetSubKey))
            {
                RegistryKey.OpenRemoteBaseKey(baseKey, targetDevice).DeleteSubKeyTree(targetSubKey, false);
                _logger.LogMessage($"Removed registry key {baseKey}\\{targetSubKey} on device {targetDevice}");
                ResultConsole.Instance.AddConsoleLine($"Removed registry key {baseKey}\\{targetSubKey} on device {targetDevice}");
                return;
            }

            ResultConsole.Instance.AddConsoleLine($"Unable to remove remote registry subkey tree on device {targetDevice}. Unable to find subkey: {baseKey}\\{targetSubKey}");
            _logger.LogWarning($"targetSubKey value must not be null or empty. DeleteSubkeyTree({targetDevice}, {baseKey}, {targetSubKey}) call contains an empty subkey.", null);
        }

        public void WriteToSubkey(string targetDevice, RegistryHive baseKey, string targetSubKey, string name, string value, RegistryValueKind keyKind)
        {
            if (string.IsNullOrWhiteSpace(targetSubKey))
            {
                ResultConsole.Instance.AddConsoleLine($"Unable to modify remote registry subkey tree on device {targetDevice}. Subkey provided was empty.");
                _logger.LogWarning($"targetSubKey value must not be null or empty. WriteToSubkey({targetDevice}, {baseKey}, {targetSubKey}, {value}) call contains an empty subkey.", null);
                return;
            }

            if (string.IsNullOrWhiteSpace(targetDevice))
            {
                ResultConsole.Instance.AddConsoleLine($"Unable to modify remote registry subkey tree. Target device argument was empty.");
                _logger.LogWarning($"targetDevice value must not be null or empty. WriteToSubkey({targetDevice}, {baseKey}, {targetSubKey}, {value}) call contains an empty subkey.", null);
                return;
            }

            var regKey = GetRegistryKey(targetDevice, RegistryHive.LocalMachine, targetSubKey);
            if (regKey != null)
            {
                regKey.SetValue(name, value, keyKind);
                regKey.Close();
                _logger.LogMessage($"Wrote registry value {value} to {name} in registry subkey {targetSubKey} on device {targetDevice}");
            }
        }
    }
}