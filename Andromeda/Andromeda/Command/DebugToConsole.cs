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

        public DebugToConsole()
        {
            ActionName = "Debug to Console";
            Desctiption = "For testing new commands, actions, and abilities.";
            Category = ActionGroup.Debug;
            netconn = new NetworkConnections();
        }

        public override void RunCommand(string input)
        {
            foreach (string d in ParseDeviceList(input))
            {
                if (netconn.CheckWMIAccessible(d, "\\root\\CIMV2"))
                {
                    ManagementScope testscope = netconn.ConnectToRemoteWMI(d, "\\root\\CIMV2");
                    if (testscope != null)
                    {
                        ResultConsole.AddConsoleLine("Successfull connection");
                    }
                }
            }
        }
    }
}
