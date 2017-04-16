using System;
using System.Collections.Generic;
using System.Management;
using System.Threading.Tasks;
using AndromedaCore.Infrastructure;
using AndromedaCore.Managers;
using Action = AndromedaCore.Action;

namespace AndromedaActions.Command
{
    public class ForceReboot : Action
    {
        private readonly IWmiServices _wmiServices;

        public ForceReboot(ILoggerService logger, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices, IWmiServices wmiServices) 
            : base(logger, networkServices, fileAndFolderServices)
        {
            ActionName = "Force Reboot";
            Description = "Force reboots the remote computer.";
            Category = "Windows Management";

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
                        Logger.LogWarning($"There was an error connecting to WMI namespace on {device}", null);
                        ResultConsole.AddConsoleLine($"There was an error connecting to WMI namespace on {device}");
                    }
                });
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