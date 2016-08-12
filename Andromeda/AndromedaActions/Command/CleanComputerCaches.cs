using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AndromedaCore;
using AndromedaCore.Infrastructure;
using AndromedaCore.Managers;
using Action = AndromedaCore.Action;

namespace AndromedaActions.Command
{
    public class CleanComputerCaches : Action
    {
        private const string CcmCachePath = "\\Windows\\ccmcache";
        private const string UsersDirectory = "\\users";
        private const string UserTemp = "\\AppData\\Local\\Temp";
        private const string UserTempInternetFiles = "\\AppData\\Local\\Microsoft\\Windows\\Temporary Internet Files";
        private const string WindowsTemp = "\\Windows\\Temp";
        private const string JavaCache = "\\AppData\\LocalLow\\Sun\\Java\\Deployment\\cache";
        
        public CleanComputerCaches(ILoggerService logger, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices) : base(logger, networkServices, fileAndFolderServices)
        {
            ActionName = "Clean Computer Temp/Cache Files";
            Description = "Cleans the temp and caches files on a remote device.";
            Category = ActionGroup.Maintenance;
        }

        public override void RunCommand(string rawDeviceList)
        {
            var devlist = ParseDeviceList(rawDeviceList);
            var failedlist = new List<string>();

            try
            {
                Parallel.ForEach(devlist, (device) =>
                {
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    if (!NetworkServices.VerifyDeviceConnectivity(device))
                    {
                        failedlist.Add(device);
                        ResultConsole.Instance.AddConsoleLine($"Device {device} failed connection verification. Added to failed list.");
                        return;
                    }

                    if (FileAndFolderServices.ValidateDirectoryExists(device, CcmCachePath, ActionName, Logger))
                    {
                        FileAndFolderServices.CleanDirectory(device, CcmCachePath, Logger);
                    }

                    if (FileAndFolderServices.ValidateDirectoryExists(device, WindowsTemp, ActionName, Logger))
                    {
                        FileAndFolderServices.CleanDirectory(device, WindowsTemp, Logger);
                    }

                    var userDirPaths = Directory.EnumerateDirectories($"\\\\{device}\\C${UsersDirectory}").ToList();
                    var userFolders = new List<string>();

                    // Create useable paths
                    foreach (var userDir in userDirPaths)
                    {
                        var cleanedPath = $"{UsersDirectory}\\{userDir.Substring(userDir.LastIndexOf("\\") + 1)}";
                        userFolders.Add(cleanedPath);
                    }

                    foreach (var userFolder in userFolders)
                    {
                        if (FileAndFolderServices.ValidateDirectoryExists(device, userFolder, ActionName, Logger))
                        {
                            // Validate and Clean User Temp Folder at "C:\users\[user]\appdata\local\temp"
                            if (FileAndFolderServices.ValidateDirectoryExists(device, userFolder + UserTemp, ActionName, Logger))
                            {
                                FileAndFolderServices.CleanDirectory(device, userFolder + UserTemp, Logger);
                            }

                            // Validate and Clean User Temporary Internet Files at "C:\Users\[user]\AppData\Local\Microsoft\Windows\Temporary Internet Files"
                            if (FileAndFolderServices.ValidateDirectoryExists(device, userFolder + UserTempInternetFiles, ActionName, Logger))
                            {
                                FileAndFolderServices.CleanDirectory(device, userFolder + UserTempInternetFiles, Logger);
                            }

                            // Validate and Clean User Java Cache Files at "C:\Users\[user]\AppData\Local\Microsoft\Windows\Temporary Internet Files"
                            if (FileAndFolderServices.ValidateDirectoryExists(device, userFolder + JavaCache, ActionName, Logger))
                            {
                                FileAndFolderServices.CleanDirectory(device, userFolder + JavaCache, Logger);
                            }
                        }
                    }
                });
                
            }
            catch (OperationCanceledException e)
            {
                ResetCancelToken(ActionName, e);
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}