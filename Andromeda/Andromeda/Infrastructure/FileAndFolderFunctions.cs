using System;
using System.IO;

namespace Andromeda.Infrastructure
{
    public static class FileAndFolderFunctions
    {
        public static void CleanDirectory(string device, string path)
        {
            var fullPath = "\\\\" + device + "\\C$" + path;

            try
            {
                Directory.Delete(fullPath, true);
                Logger.Log("Cleaned directory " + fullPath);
            }
            catch (Exception ex)
            {
                ResultConsole.Instance.AddConsoleLine("Failed to clean directory " + fullPath + ". Due to exception " + ex.Message);
                Logger.Log("Failed to clean directory " + fullPath + ". Due to exception " + ex.Message + " Inner exception: " + ex.InnerException);
            }
        }

        public static bool ValidateDirectoryExists(string device, string path, string actionName)
        {
            try
            {
                return Directory.Exists("\\\\" + device + "\\C$\\" + path);
            }
            catch (Exception ex)
            {
                ResultConsole.Instance.AddConsoleLine("There was an exception when validating the directory" + path + " for machine: " + device);
                ResultConsole.Instance.AddConsoleLine(ex.Message);
                Logger.Log(actionName + " failed to validate directory: \\\\" + device + "\\C$\\" + path);
                return false;
            }
        }

        public static bool ValidateFileExists(string device, string path, string actionName)
        {
            try
            {
                return File.Exists("\\\\" + device + "\\C$" + path);
            }
            catch (Exception ex)
            {
                ResultConsole.Instance.AddConsoleLine("There was an exception when validating the file" + path + " for machine: " + device);
                ResultConsole.Instance.AddConsoleLine(ex.Message);
                Logger.Log(actionName + " failed to validate file: \\\\" + device + "\\C$\\" + path);
                return false;
            }
        }
    }
}