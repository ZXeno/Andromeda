using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Andromeda_Actions_Core.Infrastructure;

namespace Andromeda_Actions_Core.Command
{
    public class DeleteGpoCache : Action
    {
        private const string GpoCacheDir = "\\ProgramData\\Microsoft\\Group Policy\\History";
        private readonly IPsExecServices _psExecServices;

        public DeleteGpoCache(ILoggerService logger, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices, IPsExecServices psExecServices)
            : base(logger, networkServices, fileAndFolderServices)
        {
            ActionName = "Delete GPO Cache";
            Description = "Deletes the GPO cache of the remote computer(s) and forces GPUpdate.";
            Category = ActionGroup.Other;

            _psExecServices = psExecServices;
        }

        public override void RunCommand(string rawDeviceList)
        {
            var devlist = ParseDeviceList(rawDeviceList);
            var failedlist = new List<string>();

            try
            {
                foreach (var device in devlist)
                {
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    if (!NetworkServices.VerifyDeviceConnectivity(device))
                    {
                        failedlist.Add(device);
                        ResultConsole.Instance.AddConsoleLine($"Device {device} failed connection verification. Added to failed list.");
                        continue;
                    }

                    if (FileAndFolderServices.ValidateDirectoryExists(device, GpoCacheDir, ActionName, Logger))
                    {
                        var dirtyContents = Directory.EnumerateDirectories($"\\\\{device}\\C${GpoCacheDir}").ToList();
                        var contents = new List<string>();

                        foreach (var directory in dirtyContents)
                        {
                            var cleanedPath = $"{GpoCacheDir}\\{directory.Substring(directory.LastIndexOf("\\") + 1)}";
                            contents.Add(cleanedPath);
                        }

                        foreach (var dir in contents)
                        {
                            FileAndFolderServices.CleanDirectory(device, dir, Logger);
                        }

                        _psExecServices.RunOnDeviceWithoutAuthentication(device, "cmd.exe /C gpupdate.exe /force");
                    }
                    else
                    {
                        ResultConsole.AddConsoleLine(
                            "Unable to validate the directory for the GPO cache. Please delete manually.");
                    }
                }
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