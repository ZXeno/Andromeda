using System.Collections.Generic;
using Andromeda.Model;

namespace Andromeda.Logic.Command
{
    public class RunGpUpdate : Action
    {

        public RunGpUpdate()
        {
            ActionName = "Force Group Policy Update";
            Description = "Forces a GPUpdate on the machine(s).";
            Category = ActionGroup.Other;
        }

        public override void RunCommand(string rawDeviceList)
        {
            List<string> devlist = ParseDeviceList(rawDeviceList);
            List<string> confirmedConnectionList = GetPingableDevices.GetDevices(devlist);
            List<string> failedlist = new List<string>();

            foreach (var device in confirmedConnectionList)
            {
                RunPSExecCommand.RunOnDeviceWithoutAuthentication(device, "cmd.exe /C gpupdate.exe /force");
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}