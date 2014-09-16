using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andromeda.Command
{
    class FixCEDeviceID : Action
    {
        private NetworkConnections netconn;

        public FixCEDeviceID()
        {
            ActionName = "Fix CE DeviceID XML File";
            Desctiption = "Repairs the DeviceID.XML file for a given device.";
            Category = ActionGroup.Other;
            netconn = new NetworkConnections();
        }

        public override string RunCommand(string a)
        {
            string returnMsg = "";


            return returnMsg;
        }
    }
}
