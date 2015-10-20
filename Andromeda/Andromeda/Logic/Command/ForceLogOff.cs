﻿using System;
using System.Collections.Generic;
using System.Management;
using Andromeda.Model;

namespace Andromeda.Logic.Command
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
        
        private readonly ConnectionOptions _connOps;

        public ForceLogOff()
        {
            ActionName = "Force Log Off";
            Description = "Forces the remote user to log off.";
            Category = ActionGroup.Other;
            _connOps = new ConnectionOptions();
        }

        public override void RunCommand(string rawDeviceList)
        {
            string scope = "\\root\\cimv2";
            _connOps.EnablePrivileges = true;

            List<string> devlist = ParseDeviceList(rawDeviceList);
            List<string> confirmedConnectionList = GetPingableDevices.GetDevices(devlist);
            List<string> failedlist = new List<string>();

            UpdateProgressBarForFailedConnections(devlist, confirmedConnectionList);

            foreach (var device in confirmedConnectionList)
            {
                var remote = WMIFuncs.ConnectToRemoteWMI(device, scope, _connOps);
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
                        inParams["Flags"] = 4;

                        try
                        {
                            // Execute the method and obtain the return values.
                            ManagementBaseObject outParams = ro.InvokeMethod("Win32Shutdown", inParams, null);

                            ResultConsole.AddConsoleLine("Returned with value " + WMIFuncs.GetProcessReturnValueText(Convert.ToInt32(outParams["ReturnValue"])));
                        }
                        catch (Exception e)
                        {
                            ResultConsole.AddConsoleLine("Error running " + ActionName + " due to a .Net ManagementExcept error. There are likely no users logged on!");
                            Logger.Log("Error running " + ActionName + " due to a .Net ManagementExcept error: " + e.Message);
                        }
                    }
                }
                else
                {
                    Logger.Log("There was an error connecting to WMI namespace on " + device);
                    ResultConsole.AddConsoleLine("There was an error connecting to WMI namespace on " + device);
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