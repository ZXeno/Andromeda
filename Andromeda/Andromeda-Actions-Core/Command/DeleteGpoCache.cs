using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Andromeda_Actions_Core.Infrastructure;

namespace Andromeda_Actions_Core.Command
{
    public class DeleteGpoCache : Action
    {
        private const string GpoCacheDir = "\\ProgramData\\Application Data\\Microsoft\\Group Policy\\History";

        public DeleteGpoCache()
        {
            ActionName = "Delete GPO Cache";
            Description = "Deletes the GPO cache of the remote computer(s) and forces GPUpdate.";
            Category = ActionGroup.Other;
        }

        public override void RunCommand(string rawDeviceList)
        {
            List<string> devlist = ParseDeviceList(rawDeviceList);
            List<string> failedlist = new List<string>();

            try
            {
                foreach (var device in devlist)
                {
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    if (!VerifyDeviceConnectivity(device))
                    {
                        failedlist.Add(device);
                        ResultConsole.Instance.AddConsoleLine("Device " + device +
                                                              " failed connection verification. Added to failed list.");
                        continue;
                    }

                    if (FileAndFolderFunctions.ValidateDirectoryExists(device, GpoCacheDir, ActionName))
                    {
                        List<string> dirtyContents =
                            Directory.EnumerateDirectories("\\\\" + device + "\\C$" + GpoCacheDir).ToList();
                        List<string> contents = new List<string>();

                        foreach (var directory in dirtyContents)
                        {
                            var cleanedPath = GpoCacheDir + "\\" + directory.Substring(directory.LastIndexOf("\\") + 1);
                            contents.Add(cleanedPath);
                        }

                        foreach (var dir in contents)
                        {
                            FileAndFolderFunctions.CleanDirectory(device, dir);
                        }

                        RunPsExecCommand.RunOnDeviceWithoutAuthentication(device, "cmd.exe /C gpupdate.exe /force");
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
                ResultConsole.AddConsoleLine("Operation " + ActionName + " canceled.");
                Logger.Log("Operation " + ActionName + " canceled by user request. " + e.Message);
                ResetCancelToken();
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}