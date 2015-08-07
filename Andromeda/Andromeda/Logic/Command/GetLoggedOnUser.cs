using System;
using System.Collections.Generic;
using System.Text;
using System.Management;

namespace Andromeda.Command
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

        public override void RunCommand(string a)
        {
            string scope = "\\root\\cimv2";

            List<string> devList = ParseDeviceList(a);
            List<string> successList = GetPingableDevices.GetDevices(devList);

            foreach (var d in successList)
            {
                var remote = WMIFuncs.ConnectToRemoteWMI(d, scope, _connOps);
                if (remote != null)
                {
                    ObjectQuery query = new ObjectQuery("SELECT username FROM Win32_ComputerSystem");

                    ManagementObjectSearcher searcher = new ManagementObjectSearcher(remote, query);
                    ManagementObjectCollection queryCollection = searcher.Get();

                    foreach (var resultobject in queryCollection)
                    {
                        var result = resultobject["username"] + " logged in to " + d;

                        if (result == " logged in to " + d || result == "  logged in to " + d)
                        {
                            result = "There are no users logged in to " + d + "!";
                        }

                        ResultConsole.AddConsoleLine(result);
                    }
                }
                else
                {
                    Logger.Log("There was an error connecting to WMI namespace on " + d);
                    ResultConsole.AddConsoleLine("There was an error connecting to WMI namespace on " + d);
                }
            }

        }
    }
}
