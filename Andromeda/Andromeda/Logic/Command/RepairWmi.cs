using System.Collections.Generic;
using Andromeda.Model;

namespace Andromeda.Logic.Command
{
    public class RepairWmi : Action
    {

        public RepairWmi()
        {
            ActionName = "Repair WMI";
            Description = "Repairs the WMI of the device(s).";
            Category = ActionGroup.Other;
        }

        public override void RunCommand(string rawDeviceList)
        {
            List<string> devlist = ParseDeviceList(rawDeviceList);
            List<string> confirmedConnectionList = GetPingableDevices.GetDevices(devlist);

            var failedList = new List<string>();

            foreach (var device in confirmedConnectionList)
            {
                var result = WMIFuncs.RepairRemoteWmi(device);

                if (!result)
                {
                    failedList.Add(device);
                }

                if (failedList.Count > 0)
                {
                    WriteToFailedLog(ActionName, failedList);
                }
                
            }
        }
    }
}