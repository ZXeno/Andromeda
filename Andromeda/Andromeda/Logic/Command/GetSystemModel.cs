using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using Andromeda.Infrastructure;
using Andromeda.Model;

namespace Andromeda.Logic.Command
{
    public class GetSystemModel : Action
    {
        private readonly ConnectionOptions _connOps;

        public GetSystemModel()
        {
            ActionName = "Get Device Model ID";
            Description = "Gets the model ID of the selected device.";
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

            foreach (var d in confirmedConnectionList)
            {
                var remote = WMIFuncs.ConnectToRemoteWMI(d, scope, _connOps);
                if (remote != null)
                {
                    ObjectQuery query = new SelectQuery("Win32_ComputerSystem");

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
                        continue;
                    }


                    if (queryCollection == null || queryCollection.Count == 0)
                    {
                        Logger.Log("Query returned null or empty result list for device " + d);
                        ResultConsole.AddConsoleLine("Query returned null or empty result list for device " + d);
                        continue;
                    }

                    foreach (ManagementObject resultobject in queryCollection)
                    {
                        ResultConsole.AddConsoleLine(d + " returned model ID " + resultobject["Model"]);
                    }


                }
                else
                {
                    Logger.Log("There was an error connecting to WMI namespace on " + d);
                    ResultConsole.AddConsoleLine("There was an error connecting to WMI namespace on " + d);
                }
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}