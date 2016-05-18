using System;
using System.Collections.Generic;
using Andromeda_Actions_Core.Infrastructure;

namespace Andromeda_Actions_Core.Command
{
    public class RepairWmi : Action
    {

        public RepairWmi()
        {
            ActionName = "Repair WMI";
            Description = "Repairs the WMI of the device(s).";
            Category = ActionGroup.WindowsManagement;
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
                        ResultConsole.Instance.AddConsoleLine("Device " + device + " failed connection verification. Added to failed list.");
                        continue;
                    }

                    var result = WMIFuncs.RepairRemoteWmi(device);

                    if (!result)
                    {
                        failedlist.Add(device);
                    }

                    if (failedlist.Count > 0)
                    {
                        WriteToFailedLog(ActionName, failedlist);
                    }
                }
            }
            catch (OperationCanceledException e)
            {
                ResultConsole.AddConsoleLine("Operation " + ActionName + " canceled.");
                Logger.Log("Operation " + ActionName + " canceled by user request. " + e.Message);
                ResetCancelToken();
            }
        }
    }
}