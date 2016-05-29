using System;
using System.Collections.Generic;
using Andromeda_Actions_Core.Infrastructure;

namespace Andromeda_Actions_Core.Command
{
    public class RepairWmi : Action
    {
        private readonly IWmiServices _wmiServices;

        public RepairWmi(INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices, IWmiServices wmiServices) : base(networkServices, fileAndFolderServices)
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
            }
            catch (OperationCanceledException e)
            {
                ResultConsole.AddConsoleLine($"Operation {ActionName} canceled.");
                Logger.Log($"Operation {ActionName} canceled by user request. {e.Message}");
                ResetCancelToken();
            }
        }
    }
}