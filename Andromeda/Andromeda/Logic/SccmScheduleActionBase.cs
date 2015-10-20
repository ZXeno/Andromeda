using System.Collections.Generic;
using System.Management;
using Andromeda.Model;

namespace Andromeda.Logic
{
    public class SccmScheduleActionBase : Logic.Action
    {
        protected ConnectionOptions Connection;
        protected string FailedLog = "sccm_schedule_failed_log.txt";
        protected string Scope = "\\root\\ccm:SMS_Client";

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

                ProgressData.OnUpdateProgressBar(1);
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}