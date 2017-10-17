using System;
using System.Collections.Generic;
using AndromedaCore.Infrastructure;
using AndromedaCore.Managers;
using AndromedaCore.Model;
using Action = AndromedaCore.Action;

namespace AndromedaActions.Command
{
    public class RepairWmi : Action
    {
        private readonly IWmiServices _wmiServices;

        private List<string> _parsedListCache;
        private CredToken _credToken;

        public RepairWmi(ILoggerService logger, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices, IWmiServices wmiServices) 
            : base(logger, networkServices, fileAndFolderServices)
        {
            ActionName = "Repair WMI";
            Description = "Repairs the WMI of the device(s).";
            Category = "Windows Management";
            RequiresCredentials = true;

            UiCallback = CallbackMethod;
            HasUserInterfaceElement = true;

            _wmiServices = wmiServices;
        }

        public override void OpenUserInterfaceElement(string rawDeviceList)
        {
            _parsedListCache = ParseDeviceList(rawDeviceList);

            _credToken = CredentialManager.RequestCredentials();

            if (_credToken == null)
            {
                var msg = $"Action {ActionName} canceled by user.";
                Logger.LogMessage(msg);
                ResultConsole.AddConsoleLine(msg);
                CancellationToken.Cancel();
            }
        }

        private void CallbackMethod()
        {
            var failedlist = new List<string>();

            try
            {
                foreach (var device in _parsedListCache)
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
                        var result = _wmiServices.RepairRemoteWmi(device, _credToken);

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

            _credToken.Dispose();
        }

        public override void RunCommand(string rawDeviceList)
        {
            throw new NotImplementedException($"{ActionName} requires credentials and does not utilize the RunCommand method interface.");
        }
    }
}