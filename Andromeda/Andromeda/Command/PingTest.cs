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
            return netConn.PingTest(host);
        }

        public override Dictionary<string,string> RunCommand(string[] host)
        {
            return netConn.PingTest(host);
        }

        public override Dictionary<string, string> RunCommand(List<string> host)
        {
            return netConn.PingTest(host);
        }
    }
}
