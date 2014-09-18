using System;
using System.Collections.Generic;
using System.Text;
using System.Management;

namespace Andromeda.Command
{
    class GetLoggedOnUser : Action
    {
        NetworkConnections netConn;
        ConnectionOptions connOps;

        public GetLoggedOnUser()
        {
            ActionName = "Get Logged On User";
            Desctiption = "Gets the logged in user of a remote system.";
            Category = ActionGroup.Other;
            netConn = new NetworkConnections();
            connOps = new ConnectionOptions();
        }

        public override void RunCommand(string a)
        {
            List<string> devList = ParseDeviceList(a);

            
        }
    }
}
