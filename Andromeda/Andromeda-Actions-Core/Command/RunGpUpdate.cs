using System;
using System.Collections.Generic;
using Andromeda_Actions_Core.Infrastructure;

namespace Andromeda_Actions_Core.Command
{
    public class RunGpUpdate : Action
    {
        public RunGpUpdate()
        {
            ActionName = "Force Group Policy Update";
            Description = "Forces a GPUpdate on the machine(s).";
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

                    RunPsExecCommand.RunOnDeviceWithoutAuthentication(device, "cmd.exe /C echo n | gpupdate.exe /force");
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