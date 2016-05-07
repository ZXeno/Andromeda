using System;
using System.Collections.Generic;
using System.Management;
using Andromeda_Actions_Core.Infrastructure;

namespace Andromeda_Actions_Core.Command
{
    public class GetLoggedOnUser : Action
    {
        //private CredToken _creds;
        private ConnectionOptions _connOps;

        public GetLoggedOnUser()
        {
            ActionName = "Get Logged On User";
            Description = "Gets the logged in user of a remote system.";
            Category = ActionGroup.Other;
            _connOps = new ConnectionOptions();
        }

        public override void RunCommand(string rawDeviceList)
        {
            string scope = "\\root\\cimv2";

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
                        ObjectQuery query = new ObjectQuery("SELECT username FROM Win32_ComputerSystem");

                        ManagementObjectSearcher searcher = new ManagementObjectSearcher(remote, query);
                        ManagementObjectCollection queryCollection = searcher.Get();

                        foreach (var resultobject in queryCollection)
                        {
                            var result = resultobject["username"] + " logged in to " + device;

                            if (result == " logged in to " + device || result == "  logged in to " + device)
                            {
                                result = "There are no users logged in to " + device + "!";
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
