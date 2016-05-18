using System;
using System.Collections.Generic;
using System.Management;
using System.Threading.Tasks;
using Andromeda_Actions_Core.Infrastructure;

namespace Andromeda_Actions_Core.Command
{
    public class ForceReboot : Action
    {
        //' Flag values:
        //' 0 - Log off
        //' 4 - Forced log off
        //' 1 - Shut down
        //' 5 - Forced shut down
        //' 2 - Reboot
        //' 6 - Forced reboot
        //' 8 - Power off
        //' 12 - Forced power off 

        private readonly ConnectionOptions _connOps;

        public ForceReboot()
        {
            ActionName = "Force Reboot";
            Description = "Force reboots the remote computer.";
            Category = ActionGroup.Other;
            _connOps = new ConnectionOptions();
        }

        public override void RunCommand(string rawDeviceList)
        {
            string scope = "\\root\\cimv2";
            _connOps.EnablePrivileges = true;

            List<string> devlist = ParseDeviceList(rawDeviceList);
            List<string> failedlist = new List<string>();

            try
            {
                Parallel.ForEach(devlist, (device) =>
                {
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    if (!VerifyDeviceConnectivity(device))
                    {
                        failedlist.Add(device);
                        ResultConsole.Instance.AddConsoleLine("Device " + device + " failed connection verification. Added to failed list.");
                        return;
                    }

                    var remote = WMIFuncs.ConnectToRemoteWMI(device, scope, _connOps);
                    if (remote != null)
                    {
                        WMIFuncs.ForceRebootRemoteDevice(device, remote);
                    }
                    else
                    {
                        Logger.Log("There was an error connecting to WMI namespace on " + device);
                        ResultConsole.AddConsoleLine("There was an error connecting to WMI namespace on " + device);
                    }
                });
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