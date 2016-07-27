using System;
using System.Collections.Generic;
using AndromedaCore;
using AndromedaCore.Infrastructure;
using AndromedaCore.Managers;
using Action = AndromedaCore.Action;

namespace AndromedaActions.Command
{
    public class RunGpUpdate : Action
    {
        private readonly IPsExecServices _psExecServices;

        public RunGpUpdate(ILoggerService logger, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices, IPsExecServices psExecServices) 
            : base(logger, networkServices, fileAndFolderServices)
        {
            ActionName = "Force Group Policy Update";
            Description = "Forces a GPUpdate on the machine(s).";
            Category = ActionGroup.WindowsManagement;

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
                ResetCancelToken(ActionName, e);
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}