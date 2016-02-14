using System.Collections.Generic;
using System.IO;
using System.Linq;
using Andromeda.Infrastructure;
using Andromeda.Model;

namespace Andromeda.Logic.Command
{
    public class CleanComputerCaches : Action
    {
        private const string CcmCachePath = "\\Windows\\ccmcache";
        private const string UsersDirectory = "\\users";
        private const string UserTemp = "\\AppData\\Local\\Temp";
        private const string UserTempInternetFiles = "\\AppData\\Local\\Microsoft\\Windows\\Temporary Internet Files";
        private const string WindowsTemp = "\\Windows\\Temp";
        private const string JavaCache = "\\AppData\\LocalLow\\Sun\\Java\\Deployment\\cache";

        public CleanComputerCaches()
        {
            ActionName = "Clean Computer Temp/Cache Files";
            Description = "Cleans the temp and caches files on a remote device.";
            Category = ActionGroup.Maintenance;
        }

        public override void RunCommand(string rawDeviceList)
        {
            List<string> devlist = ParseDeviceList(rawDeviceList);
            List<string> confirmedConnectionList = GetPingableDevices.GetDevices(devlist);
            List<string> failedlist = new List<string>();
            
            foreach (var device in confirmedConnectionList)
            {
                if (FileAndFolderFunctions.ValidateDirectoryExists(device, CcmCachePath, ActionName))
                {
                    FileAndFolderFunctions.CleanDirectory(device, CcmCachePath);
                }

                if (FileAndFolderFunctions.ValidateDirectoryExists(device, WindowsTemp, ActionName))
                {
                    FileAndFolderFunctions.CleanDirectory(device, WindowsTemp);
                }

                List<string> userDirPaths = Directory.EnumerateDirectories("\\\\" + device + "\\C$" + UsersDirectory).ToList();
                List<string> userFolders = new List<string>();

                // Create useable paths
                foreach (var userDir in userDirPaths)
                {
                    var cleanedPath = UsersDirectory + "\\" + userDir.Substring(userDir.LastIndexOf("\\") + 1);
                    userFolders.Add(cleanedPath);
                }

                foreach (var userFolder in userFolders)
                {
                    if (FileAndFolderFunctions.ValidateDirectoryExists(device, userFolder, ActionName))
                    {
                        // Validate and Clean User Temp Folder at "C:\users\[user]\appdata\local\temp"
                        if (FileAndFolderFunctions.ValidateDirectoryExists(device, userFolder + UserTemp, ActionName))
                        {
                            FileAndFolderFunctions.CleanDirectory(device, userFolder + UserTemp);
                        }

                        // Validate and Clean User Temporary Internet Files at "C:\Users\[user]\AppData\Local\Microsoft\Windows\Temporary Internet Files"
                        if (FileAndFolderFunctions.ValidateDirectoryExists(device, userFolder + UserTempInternetFiles, ActionName))
                        {
                            FileAndFolderFunctions.CleanDirectory(device, userFolder + UserTempInternetFiles);
                        }

                        // Validate and Clean User Java Cache Files at "C:\Users\[user]\AppData\Local\Microsoft\Windows\Temporary Internet Files"
                        if (FileAndFolderFunctions.ValidateDirectoryExists(device, userFolder + JavaCache, ActionName))
                        {
                            FileAndFolderFunctions.CleanDirectory(device, userFolder + JavaCache);
                        }
                    }
                }
                
            }

            if (failedlist.Count > 0)
            {
                Logger.WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}