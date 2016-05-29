using System;
using System.Collections.Generic;
using System.Management;
using System.Threading.Tasks;
using Andromeda_Actions_Core.Infrastructure;

namespace Andromeda_Actions_Core.Command
{
    public class ForceReboot : Action
    {
        private readonly IWmiServices _wmiServices;

        public ForceReboot(INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices, IWmiServices wmiServices) : base(networkServices, fileAndFolderServices)
        {
            ActionName = "Force Reboot";
            Description = "Force reboots the remote computer.";
            Category = ActionGroup.Other;

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
                Parallel.ForEach(devlist, (device) =>
                {
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    if (!NetworkServices.VerifyDeviceConnectivity(device))
                    {
                        failedlist.Add(device);
                        ResultConsole.Instance.AddConsoleLine($"Device {device} failed connection verification. Added to failed list.");
                        return;
                    }

                    var remote = _wmiServices.ConnectToRemoteWmi(device, _wmiServices.RootNamespace, connOps);
                    if (remote != null)
                    {
                        _wmiServices.ForceRebootRemoteDevice(device, remote);
                    }
                    else
                    {
                        Logger.Log($"There was an error connecting to WMI namespace on {device}");
                        ResultConsole.AddConsoleLine($"There was an error connecting to WMI namespace on {device}");
                    }
                });
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