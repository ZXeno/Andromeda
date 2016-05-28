using System;
using System.Collections.Generic;
using System.Management;
using Andromeda_Actions_Core.Infrastructure;

namespace Andromeda_Actions_Core.Command
{
    public class GetLoggedOnUser : Action
    {
        private const string Scope = "\\root\\cimv2";

        public GetLoggedOnUser()
        {
            ActionName = "Get Logged On User";
            Description = "Gets the logged in user of a remote system.";
            Category = ActionGroup.Other;
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

                    if (!VerifyDeviceConnectivity(device))
                    {
                        failedlist.Add(device);
                        ResultConsole.Instance.AddConsoleLine($"Device {device} failed connection verification. Added to failed list.");
                        continue;
                    }

                    var remote = WMIFuncs.ConnectToRemoteWMI(device, Scope, new ConnectionOptions());
                    if (remote != null)
                    {
                        var query = new ObjectQuery("SELECT username FROM Win32_ComputerSystem");

                        var searcher = new ManagementObjectSearcher(remote, query);
                        var queryCollection = searcher.Get();

                        foreach (var resultobject in queryCollection)
                        {
                            var result = $"{resultobject["username"]} logged in to {device}";

                            if (result == $" logged in to {device}" || result == $"  logged in to {device}")
                            {
                                result = $"There are no users logged in to {device}!";
                            }

                            ResultConsole.AddConsoleLine(result);
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
