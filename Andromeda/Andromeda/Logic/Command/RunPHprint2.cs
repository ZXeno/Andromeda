using System.Collections.Generic;
using System.Management;

namespace Andromeda.Command
{
    public class RunPHprint2 : Action
    {
        public RunPHprint2()
        {
            ActionName = "Run PHPrint2 Remotely";
            Description = "Run phprint2.exe remotely, as the remote machine's current user. Or a \"force printer remap\".";
            Category = ActionGroup.Other;
        }

        public override void RunCommand(string deviceList)
        {
            List<string> devlist = ParseDeviceList(deviceList);
            List<string> successList = GetPingableDevices.GetDevices(devlist);

            foreach (var d in successList)
            {
                //RunPSExecCommand.RunOnDeviceWithoutAuthentication(d, @"WMIC /node:" + d + @" process create call phprint2.exe");
                RunPSExecCommand.RunOnDeviceWithoutAuthentication(d, @"c:\windows\phprint2.exe");
                WMIFuncs.RunCommand(d, @"c:\windows\phprint2.exe", null);
                // @"WMIC process create call phprint2.exe"
            }
        }
    }
}