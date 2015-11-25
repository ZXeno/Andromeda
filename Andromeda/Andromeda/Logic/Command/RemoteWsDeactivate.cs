using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Andromeda.Model;

namespace Andromeda.Logic.Command
{
    public class RemoteWsDeactivate : Action
    {

        private const string RemoteAdminDir = "windows";
        private const string PsExecCommandLine = "c:\\Windows\\WSDeactivate64.exe -s";
        private string WsDeactivateExecutable = "WSDeactivate64.exe";
        private readonly string _pathToWsDeactivate;

        public RemoteWsDeactivate()
        {
            ActionName = "Remote WSDeactivate (x64 ONLY)";
            Description = "Silently runs WSDeactivate on the remote machine.";
            Category = ActionGroup.Other;

            _pathToWsDeactivate = ConfigManager.CurrentConfig.ComponentDirectory + "\\" + WsDeactivateExecutable;
        }

        public override void RunCommand(string rawDeviceList)
        {
            List<string> devlist = ParseDeviceList(rawDeviceList);
            List<string> confirmedConnectionList = GetPingableDevices.GetDevices(devlist);
            List<string> failedlist = new List<string>();

            UpdateProgressBarForFailedConnections(devlist, confirmedConnectionList);

            foreach (var device in confirmedConnectionList)
            {
                if (ValidateDirectoryExists(device, RemoteAdminDir))
                {
                    if (!File.Exists("\\\\" + device + "\\C$\\" + RemoteAdminDir + "\\" + WsDeactivateExecutable))
                    {
                        try
                        {
                            File.Copy(_pathToWsDeactivate, "\\\\" + device + "\\C$\\" + RemoteAdminDir + "\\" + WsDeactivateExecutable);
                        }
                        catch (Exception e)
                        {
                            Logger.Log("Exception copying WSDeactivate to remote device: " + e.Message + ". Inner exception: " + e.InnerException);
                            ResultConsole.AddConsoleLine("There was an error copying WSDeactivate to the remote machine. Please run WSDeactiavte manually.");
                            ResultConsole.AddConsoleLine("Device added to failed list");
                            failedlist.Add(device);
                            ProgressData.OnUpdateProgressBar(1);
                            continue;
                        }
                        
                    }

                    RunPSExecCommand.RunOnDeviceWithoutAuthentication(device, PsExecCommandLine);
                }
                else
                {
                    Logger.Log("Unable to validate the destination directory for WSDeactivate.exe. Destination: " + "\\\\" + device + "\\C$\\" + RemoteAdminDir + "\\" + WsDeactivateExecutable);
                    ResultConsole.AddConsoleLine("Unable to validate the destination directory for WSDeactivate.exe. Please run WSDeactivate manually.");
                    ResultConsole.AddConsoleLine("Device added to failed list");
                    failedlist.Add(device);
                }

                ProgressData.OnUpdateProgressBar(1);
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}