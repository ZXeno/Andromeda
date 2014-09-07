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

        public DebugToConsole(string name, string descriptor, ActionGroup cat)
        {
            ActionName = name;
            Desctiption = descriptor;
            Category = cat;
            netconn = new NetworkConnections();
        }

        public override string RunCommand(string input)
        {
            string sendback = "";

            foreach (string d in ParseDeviceList(input))
            {
                ManagementScope testscope = netconn.ConnectToRemoteWMI(d);
            }

            return sendback;
        }
    }
}
