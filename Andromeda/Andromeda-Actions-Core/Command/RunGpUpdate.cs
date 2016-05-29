using System;
using System.Collections.Generic;
using Andromeda_Actions_Core.Infrastructure;

namespace Andromeda_Actions_Core.Command
{
    public class RunGpUpdate : Action
    {
        private readonly IPsExecServices _psExecServices;

        public RunGpUpdate(INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices, IPsExecServices psExecServices) : base(networkServices, fileAndFolderServices)
        {
            ActionName = "Force Group Policy Update";
            Description = "Forces a GPUpdate on the machine(s).";
            Category = ActionGroup.Other;

            _psExecServices = psExecServices;
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

                    _psExecServices.RunOnDeviceWithoutAuthentication(device, "cmd.exe /C echo n | gpupdate.exe /force");
                }
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