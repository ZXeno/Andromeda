using System;
using Microsoft.Win32;

namespace Andromeda.Logic
{
    public static class RegistryFunctions
    {
        public static bool ValidateKeyExists(string targetDevice, RegistryHive baseKey, string targetSubKey)
        {
            try
            {
                RegistryKey targetkey = RegistryKey.OpenRemoteBaseKey(baseKey, targetDevice, RegistryView.Registry64).OpenSubKey(targetSubKey);

                if (targetkey != null)
                {
                    Logger.Log("Attempt to validate registry key succeeded.");
                    targetkey.Close();
                    return true;
                }

                Logger.Log("Attempt to validate registry key, but targetkey value returned null. Check the path and try again.");
                return false;
            }
            catch (Exception e)
            {
                Logger.Log("Unable to validate remote registy key " + targetSubKey + ": " + e.Message + ". Inner exception: " + e.InnerException);
                ResultConsole.Instance.AddConsoleLine("Unable to validate key " + targetSubKey + "on device " + targetDevice + ".");
                return false;
            }
        }

        public static bool ValidateKeyValueExists(RegistryKey subKey, string targetKeyValue, string expectedName)
        {
            return false;
        }

        public static RegistryKey GetRegistryKey(string targetDevice, RegistryHive baseKey, string targetSubKey)
        {
            if (string.IsNullOrEmpty(targetSubKey))
            {
                Logger.Log("targetSubKey value must not be null or empty. GetRegistryKey(" + targetDevice +", " + baseKey + ", " + targetSubKey + ") call contains an empty subkey.");
                return null;
            }

            try
            {
                
                return RegistryKey.OpenRemoteBaseKey(baseKey, targetDevice, RegistryView.Registry64).OpenSubKey(targetSubKey, true);
            }
            catch (Exception e)
            {
                Logger.Log("Unable to get remote registy key " + targetSubKey + ": " + e.Message + ". Inner exception: " + e.InnerException);
                ResultConsole.Instance.AddConsoleLine("Unable to get remote registry key " + targetSubKey + "on device " + targetDevice + ".");
                return null;
            }
        }

<<<<<<< HEAD
        public static void DeleteSubkeyTree(string targetDevice, RegistryHive baseKey, string targetSubKey)
        {
            if (string.IsNullOrEmpty(targetSubKey))
            {
                ResultConsole.Instance.AddConsoleLine("Unable to remove remote registry subkey tree on device " + targetDevice + ". Subkey provided was empty.");
                Logger.Log("targetSubKey value must not be null or empty. DeleteSubkeyTree(" + targetDevice + ", " + baseKey + ", " + targetSubKey + ") call contains an empty subkey.");
                return;
            }

            if (ValidateKeyExists(targetDevice, baseKey, targetSubKey))
            {
                RegistryKey.OpenRemoteBaseKey(baseKey, targetDevice).DeleteSubKeyTree(targetSubKey, false);
                Logger.Log("Removed registry key " + baseKey + "\\" + targetSubKey + " on device " + targetDevice);
                ResultConsole.Instance.AddConsoleLine("Removed registry key " + baseKey + "\\" + targetSubKey + " on device " + targetDevice);
                return;
            }

            ResultConsole.Instance.AddConsoleLine("Unable to remove remote registry subkey tree on device " + targetDevice + ". Unable to find subkey: " + baseKey + "\\" + targetSubKey);
            Logger.Log("targetSubKey value must not be null or empty. DeleteSubkeyTree(" + targetDevice + ", " + baseKey + ", " + targetSubKey + ") call contains an empty subkey.");
        }
=======
        
>>>>>>> e6126d7828065f238a70d12e97056df14523ff28
    }
}