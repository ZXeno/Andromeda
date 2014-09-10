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

        public PingTest()
        {
            ActionName = "Ping Test";
            Desctiption = "Runs a ping test against the device list.";
            Category = ActionGroup.Other;
            netConn = new NetworkConnections();
        }

        public override string RunCommand(string host)
        {
            string response = "";

            foreach (string d in ParseDeviceList(host))
            {
                if (d == "") { continue; }
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
