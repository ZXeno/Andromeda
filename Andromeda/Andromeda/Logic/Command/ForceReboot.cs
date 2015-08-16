using System;
using System.Collections.Generic;
using System.Management;
using Andromeda.Model;

namespace Andromeda.Command
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

        public override void RunCommand(string a)
        {
            string scope = "\\root\\cimv2";
            _connOps.EnablePrivileges = true;

            List<string> devList = ParseDeviceList(a);
            List<string> successList = GetPingableDevices.GetDevices(devList);

            foreach (var d in successList)
            {
                var remote = WMIFuncs.ConnectToRemoteWMI(d, scope, _connOps);
                if (remote != null)
                {
                    ObjectQuery query = new SelectQuery("Win32_OperatingSystem");

                    ManagementObjectSearcher searcher = new ManagementObjectSearcher(remote, query);
                    ManagementObjectCollection queryCollection = searcher.Get();

                    foreach (var resultobject in queryCollection)
                    {
                        ManagementObject ro = resultobject as ManagementObject;
                        // Obtain in-parameters for the method
                        ManagementBaseObject inParams = ro.GetMethodParameters("Win32Shutdown");

                        // Add the input parameters.
                        inParams["Flags"] = 6;

                        // Execute the method and obtain the return values.
                        ManagementBaseObject outParams = ro.InvokeMethod("Win32Shutdown", inParams, null);

                        ResultConsole.AddConsoleLine("Returned with value " + WMIFuncs.GetProcessReturnValueText(Convert.ToInt32(outParams["ReturnValue"])));
                    }
                }
                else
                {
                    Logger.Log("There was an error connecting to WMI namespace on " + d);
                    ResultConsole.AddConsoleLine("There was an error connecting to WMI namespace on " + d);
                }
            }

        }
    }
}