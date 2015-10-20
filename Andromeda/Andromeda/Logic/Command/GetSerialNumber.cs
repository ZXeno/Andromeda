using System;
using System.Collections.Generic;
using System.Management;
using Andromeda.Model;

namespace Andromeda.Logic.Command
{
    public class GetSerialNumber : Action
    {
        private readonly ConnectionOptions _connOps;

        public GetSerialNumber()
        {
            ActionName = "Get Device Serial Number";
            Description = "Gets the serial number of the selected device.";
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
                    ObjectQuery query = new SelectQuery("Win32_BIOS");

                    ManagementObjectSearcher searcher = new ManagementObjectSearcher(remote, query);

                    ManagementObjectCollection queryCollection = null;

                    try
                    {
                        queryCollection = searcher.Get();
                    }
                    catch (Exception e)
                    {
                        Logger.Log("QueryCollection returned with exception " + e.Message);
                        ResultConsole.AddConsoleLine("QueryCollection returned with exception " + e.Message);
                        ProgressData.OnUpdateProgressBar(1);
                        continue;
                    }
                    

                    if (queryCollection == null || queryCollection.Count == 0)
                    {
                        Logger.Log("Query returned null or empty result list for device " + device);
                        ResultConsole.AddConsoleLine("Query returned null or empty result list for device " + device);
                        ProgressData.OnUpdateProgressBar(1);
                        continue;
                    }

                    foreach (ManagementObject resultobject in queryCollection)
                    {
                        ResultConsole.AddConsoleLine(device + " returned serial number " + resultobject["SerialNumber"]);
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