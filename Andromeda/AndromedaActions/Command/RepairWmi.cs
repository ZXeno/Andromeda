using System;
using System.Collections.Generic;
using AndromedaCore;
using AndromedaCore.Infrastructure;
using AndromedaCore.Managers;
using Action = AndromedaCore.Action;

namespace AndromedaActions.Command
{
    public class RepairWmi : Action
    {
        private readonly IWmiServices _wmiServices;

        public RepairWmi(ILoggerService logger, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices, IWmiServices wmiServices) 
            : base(logger, networkServices, fileAndFolderServices)
        {
            ActionName = "Repair WMI";
            Description = "Repairs the WMI of the device(s).";
            Category = ActionGroup.WindowsManagement;

            _wmiServices = wmiServices;
        }

        public override void RunCommand(string rawDeviceList)
        {
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

                    try
                    {
                        var result = _wmiServices.RepairRemoteWmi(device);

                        if (!result)
                        {
                            failedlist.Add(device);
                        }

                        if (failedlist.Count > 0)
                        {
                            WriteToFailedLog(ActionName, failedlist);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"There was an error repairing WMI on device {device}. {ex.Message}", ex);
                        ResultConsole.Instance.AddConsoleLine($"There was an error repairing WMI on device {device}.");
                        failedlist.Add(device);
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