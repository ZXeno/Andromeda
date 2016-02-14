using System.Collections.Generic;
using System.Management;
using Andromeda.Infrastructure;

namespace Andromeda.Logic
{
    public class SccmScheduleActionBase : Action
    {
        protected ConnectionOptions Connection;
        protected string FailedLog = "sccm_schedule_failed_log.txt";
        protected string Scope = "\\root\\ccm:SMS_Client";

        public override void RunCommand(string rawDeviceList)
        {
            throw new System.NotImplementedException();
        }

        public virtual void RunScheduleTrigger(string scheduleId, string deviceList)
        {
            List<string> devlist = ParseDeviceList(deviceList);
            List<string> successList = GetPingableDevices.GetDevices(devlist);
            List<string> failedlist = new List<string>();
            Connection = new ConnectionOptions();
            Connection.EnablePrivileges = true;

            foreach (var d in successList)
            {
                var remote = WMIFuncs.ConnectToRemoteWMI(d, Scope, Connection);
                if (remote != null)
                {
                    SccmClientFuncs.TriggerClientAction(scheduleId, remote);
                }
                else
                {
                    ResultConsole.AddConsoleLine("Error connecting to WMI scope " + d + ". Process aborted for this device.");
                    Logger.Log("Error connecting to WMI scope " + d + ". Process aborted for this device.");
                    failedlist.Add(d);
                }

            }

            if (failedlist.Count > 0)
            {
                Logger.WriteToFailedLog(ActionName, failedlist);
            }
        }

        
    }
}