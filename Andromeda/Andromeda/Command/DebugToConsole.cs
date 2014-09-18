using System;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andromeda.Command
{
    class DebugToConsole : Action
    {
        NetworkConnections netconn;
        WMIFuncs wmi;
        ConnectionOptions connOps;

        public DebugToConsole()
        {
            ActionName = "Debug to Console";
            Desctiption = "For testing new commands, actions, and abilities.";
            Category = ActionGroup.Debug;
            netconn = new NetworkConnections();
            connOps = new ConnectionOptions();
            connOps.Impersonation = ImpersonationLevel.Impersonate;
            wmi = new WMIFuncs();
        }

        public override void RunCommand(string input)
        {
            foreach (string d in ParseDeviceList(input))
            {
                if (wmi.CheckWMIAccessible(d, "\\root\\CIMV2", connOps))
                {
                    ManagementScope testscope = wmi.ConnectToRemoteWMI(d, "\\root\\CIMV2", connOps);
                    if (testscope != null)
                    {
                        ResultConsole.AddConsoleLine("Successfull connection");
                    }
                }
            }
        }
    }
}
