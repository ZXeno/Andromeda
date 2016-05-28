using System;
using System.Collections.Generic;
using System.Management;
using Andromeda_Actions_Core.Infrastructure;

namespace Andromeda_Actions_Core.Command
{
    public class ForceLogOff : Action
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

        private const string Scope = "\\root\\cimv2";

        public ForceLogOff()
        {
            ActionName = "Force Log Off";
            Description = "Forces the remote user to log off.";
            Category = ActionGroup.Other;
        }

        public override void RunCommand(string rawDeviceList)
        {
            var devlist = ParseDeviceList(rawDeviceList);
            var failedlist = new List<string>();

            var connOps = new ConnectionOptions
            {
                EnablePrivileges = true
            };

            try
            {
                foreach (var device in devlist)
                {
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    if (!VerifyDeviceConnectivity(device))
                    {
                        failedlist.Add(device);
                        ResultConsole.Instance.AddConsoleLine($"Device {device} failed connection verification. Added to failed list.");
                        continue;
                    }

                    var remote = WMIFuncs.ConnectToRemoteWMI(device, Scope, connOps);
                    if (remote != null)
                    {
                        ObjectQuery query = new SelectQuery("Win32_OperatingSystem");

                        var searcher = new ManagementObjectSearcher(remote, query);
                        var queryCollection = searcher.Get();

                        foreach (var resultobject in queryCollection)
                        {
                            var ro = resultobject as ManagementObject;
                            // Obtain in-parameters for the method
                            var inParams = ro.GetMethodParameters("Win32Shutdown");

                            // Add the input parameters.
                            inParams["Flags"] = 4;

                            try
                            {
                                // Execute the method and obtain the return values.
                                var outParams = ro.InvokeMethod("Win32Shutdown", inParams, null);

                                ResultConsole.AddConsoleLine($"Returned with value {WMIFuncs.GetProcessReturnValueText(Convert.ToInt32(outParams["ReturnValue"]))}");
                            }
                            catch (Exception e)
                            {
                                ResultConsole.AddConsoleLine($"Error running {ActionName} due to a .Net ManagementExcept error. There are likely no users logged on!");
                                Logger.Log($"Error running {ActionName} due to a .Net ManagementExcept error: {e.Message}");
                            }
                        }
                    }
                    else
                    {
                        Logger.Log($"There was an error connecting to WMI namespace on {device}");
                        ResultConsole.AddConsoleLine($"There was an error connecting to WMI namespace on {device}");
                    }
                }
            }
            catch (OperationCanceledException e)
            {
                ResultConsole.AddConsoleLine($"Operation {ActionName} canceled.");
                Logger.Log($"Operation {ActionName} canceled by user request. {e.Message}");
                ResetCancelToken();
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}