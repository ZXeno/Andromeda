using System;
using System.Collections.Generic;
using System.Management;
using AndromedaCore;
using AndromedaCore.Infrastructure;
using AndromedaCore.Managers;
using Action = AndromedaCore.Action;

namespace AndromedaActions.Command
{
    public class GetSerialNumber : Action
    {
        private readonly IWmiServices _wmiServices;

        public GetSerialNumber(ILoggerService logger, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices, IWmiServices wmiServices) 
            : base(logger, networkServices, fileAndFolderServices)
        {
            ActionName = "Get Device Serial Number";
            Description = "Gets the serial number of the selected device.";
            Category = "Reporting";

            _wmiServices = wmiServices;
        }

        public override void RunCommand(string rawDeviceList)
        {
            
            var connOps = new ConnectionOptions
            {
                EnablePrivileges = true
            };

            var devlist = ParseDeviceList(rawDeviceList);
            var failedlist = new List<string>();

            try
            {
                foreach (var device in devlist)
                {
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    if (!NetworkServices.VerifyDeviceConnectivity(device))
                    {
                        failedlist.Add(device);
                        ResultConsole.Instance.AddConsoleLine($"Device {device} failed connection verification. Added to failed list.");
                        continue;
                    }

                    var remote = _wmiServices.ConnectToRemoteWmi(device, _wmiServices.RootNamespace, connOps);
                    if (remote != null)
                    {
                        ObjectQuery query = new SelectQuery("Win32_BIOS");

                        var searcher = new ManagementObjectSearcher(remote, query);

                        ManagementObjectCollection queryCollection = null;

                        try
                        {
                            queryCollection = searcher.Get();
                        }
                        catch (Exception e)
                        {
                            Logger.LogWarning("QueryCollection returned with exception.", e);
                            ResultConsole.AddConsoleLine($"QueryCollection returned with exception {e.Message}");
                            continue;
                        }


                        if (queryCollection == null || queryCollection.Count == 0)
                        {
                            Logger.LogWarning($"Query returned null or empty result list for device {device}", null);
                            ResultConsole.AddConsoleLine($"Query returned null or empty result list for device {device}");
                            continue;
                        }

                        foreach (ManagementObject resultobject in queryCollection)
                        {
                            ResultConsole.AddConsoleLine($"{device} returned serial number {resultobject["SerialNumber"]}");
                        }
                    }
                    else
                    {
                        Logger.LogWarning($"There was an error connecting to WMI namespace on {device}", null);
                        ResultConsole.AddConsoleLine($"There was an error connecting to WMI namespace on {device}");
                    }
                }
            }
            catch (OperationCanceledException e)
            {
                ResetCancelToken(ActionName, e);
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}