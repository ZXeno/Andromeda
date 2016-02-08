using System.Collections.Generic;
using System.IO;
using System.Linq;
using Andromeda.Model;

namespace Andromeda.Logic.Command
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
            List<string> confirmedConnectionList = GetPingableDevices.GetDevices(devlist);
            List<string> failedlist = new List<string>();

            foreach (var device in confirmedConnectionList)
            {
                if (ValidateDirectoryExists(device, GpoCacheDir))
                {
                    List<string> dirtyContents = Directory.EnumerateDirectories("\\\\" + device + "\\C$" + GpoCacheDir).ToList();
                    List<string> contents = new List<string>();

                    foreach (var directory in dirtyContents)
                    {
                        var cleanedPath = GpoCacheDir + "\\" + directory.Substring(directory.LastIndexOf("\\") + 1);
                        contents.Add(cleanedPath);
                    }

                    foreach (var dir in contents)
                    {
                        CleanDirectory(device, dir);
                    }

                    RunPSExecCommand.RunOnDeviceWithoutAuthentication(device, "cmd.exe /C gpupdate.exe /force");
                }
                else
                {
                    ResultConsole.AddConsoleLine("Unable to validate the directory for the GPO cache. Please delete manually.");
                }
                
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}