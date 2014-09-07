using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andromeda.Command
{
    public class PingTest : Action
    {
        NetworkConnections netConn;

        public PingTest(string name, string descriptor, ActionGroup cat)
        {
            ActionName = name;
            Desctiption = descriptor;
            Category = cat;
            netConn = new NetworkConnections();
        }

        public override string RunCommand(string host)
        {
            string response = "";

            foreach (string d in ParseDeviceList(host))
            {
                response += netConn.PingTest(d) + "\n";
            }

            return response;
        }

        public override string ToString()
        {
            return ActionName;
        }
    }
}
