using System.Collections.Generic;
using System.Management;
using Andromeda.Model;

namespace Andromeda.Logic.Command
{
    public class GetSerialNumber : Action
    {
        private readonly ConnectionOptions _connOps;

        public GetSerialNumber()
        {
            ActionName = "Get Device Serial Number";
            Description = "Gets the serial number of the selected device.";
            Category = ActionGroup.Other;
            _connOps = new ConnectionOptions();
        }

        public override void RunCommand(string a)
        {
            string scope = "\\root\\cimv2";
            _connOps.EnablePrivileges = true;

            List<string> devList = ParseDeviceList(a);
            List<string> successList = GetPingableDevices.GetDevices(devList);

            foreach (var d in successList)
            {
                var remote = WMIFuncs.ConnectToRemoteWMI(d, scope, _connOps);
                if (remote != null)
                {
                    ObjectQuery query = new SelectQuery("Win32_BIOS");

                    ManagementObjectSearcher searcher = new ManagementObjectSearcher(remote, query);
                    ManagementObjectCollection queryCollection = searcher.Get();

                    foreach (ManagementObject resultobject in queryCollection)
                    {
                        
                        ResultConsole.AddConsoleLine(d + " returned serial number " + resultobject["SerialNumber"]);
                    }
                }
                else
                {
                    Logger.Log("There was an error connecting to WMI namespace on " + d);
                    ResultConsole.AddConsoleLine("There was an error connecting to WMI namespace on " + d);
                }

                ProgressData.OnUpdateProgressBar(1);
            }

        }
    }
}