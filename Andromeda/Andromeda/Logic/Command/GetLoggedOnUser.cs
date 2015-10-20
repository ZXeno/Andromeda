using System.Collections.Generic;
using System.Management;
using Andromeda.Model;

namespace Andromeda.Logic.Command
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
            List<string> confirmedConnectionList = GetPingableDevices.GetDevices(devlist);
            List<string> failedlist = new List<string>();

            UpdateProgressBarForFailedConnections(devlist, confirmedConnectionList);

            foreach (var device in confirmedConnectionList)
            {
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

                ProgressData.OnUpdateProgressBar(1);
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}
