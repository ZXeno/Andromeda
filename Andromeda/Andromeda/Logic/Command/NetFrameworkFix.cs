using System;
using System.Collections.Generic;
using System.IO;
using Andromeda.Infrastructure;

namespace Andromeda.Logic.Command
{
    public class NetFrameworkFix : Action
    {

        private const string RemoteAdminDir = "windows";
        private const string PsExecCommandLine = "c:\\Windows\\NetFxRepairTool.exe /q /n";
        private string NetFxRepairToolExecutable = "NetFxRepairTool.exe";
        private readonly string _pathToNetFxRepairTool;

        public NetFrameworkFix()
        {
            ActionName = ".Net Framework Fix Microsoft Tool";
            Description = "Silently runs the Microsoft .Net Repair Tool on the remote machine(s).";
            Category = ActionGroup.Maintenance;

            _pathToNetFxRepairTool = ConfigManager.CurrentConfig.ComponentDirectory + "\\" + NetFxRepairToolExecutable;
        }

        public override void RunCommand(string rawDeviceList)
        {
            List<string> devlist = ParseDeviceList(rawDeviceList);
            List<string> confirmedConnectionList = GetPingableDevices.GetDevices(devlist);
            List<string> failedlist = new List<string>();

            foreach (var device in confirmedConnectionList)
            {
                if (FileAndFolderFunctions.ValidateDirectoryExists(device, RemoteAdminDir, ActionName))
                {
                    if (!File.Exists("\\\\" + device + "\\C$\\" + RemoteAdminDir + "\\" + NetFxRepairToolExecutable))
                    {
                        try
                        {
                            File.Copy(_pathToNetFxRepairTool, "\\\\" + device + "\\C$\\" + RemoteAdminDir + "\\" + NetFxRepairToolExecutable);
                        }
                        catch (Exception e)
                        {
                            Logger.Log("Exception copying NetFx Repair Tool to remote device: " + e.Message + ". Inner exception: " + e.InnerException);
                            ResultConsole.AddConsoleLine("There was an error copying the NetFx Repair Tool to the remote machine. Please run the tool manually.");
                            ResultConsole.AddConsoleLine("Device added to failed list");
                            failedlist.Add(device);
                            continue;
                        }

                    }

                    RunPSExecCommand.RunOnDeviceWithAuthentication(device, PsExecCommandLine, Program.CredentialManager.UserCredentials);
                }
                else
                {
                    Logger.Log("Unable to validate the destination directory for NetFxRepairTool.exe. Destination: " + "\\\\" + device + "\\C$\\" + RemoteAdminDir + "\\" + NetFxRepairToolExecutable);
                    ResultConsole.AddConsoleLine("Unable to validate the destination directory for NetFxRepairTool.exe. Please run the tool manually.");
                    ResultConsole.AddConsoleLine("Device added to failed list");
                    failedlist.Add(device);
                }
            }

            if (failedlist.Count > 0)
            {
                Logger.WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}