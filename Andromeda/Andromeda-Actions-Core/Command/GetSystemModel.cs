using System;
using System.Collections.Generic;
using System.Management;
using Andromeda_Actions_Core.Infrastructure;

namespace Andromeda_Actions_Core.Command
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

                    var remote = WMIFuncs.ConnectToRemoteWMI(device, scope, _connOps);
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
                            Logger.Log("Query returned null or empty result list for device " + device);
                            ResultConsole.AddConsoleLine("Query returned null or empty result list for device " + device);
                            continue;
                        }

                        foreach (ManagementObject resultobject in queryCollection)
                        {
                            ResultConsole.AddConsoleLine(device + " returned model ID " + resultobject["Model"]);
                        }


                    }
                    else
                    {
                        Logger.Log("There was an error connecting to WMI namespace on " + device);
                        ResultConsole.AddConsoleLine("There was an error connecting to WMI namespace on " + device);
                    }
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