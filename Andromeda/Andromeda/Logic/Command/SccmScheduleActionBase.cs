using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Text;
using System.Windows;

namespace Andromeda.Command
{
    public class SccmScheduleActionBase : Action
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
            }

            if (failedlist.Count > 0)
            {
                ResultConsole.AddConsoleLine("There were " + failedlist.Count + "computers that failed the process. They have been recorded in the log.");
                StringBuilder sb = new StringBuilder();

                if (File.Exists(Program.Config.ResultsDirectory + "\\" + FailedLog))
                {
                    File.Delete(Program.Config.ResultsDirectory + "\\" + FailedLog);
                    Logger.Log("Deleted file " + Program.Config.ResultsDirectory + "\\" + FailedLog);
                }

                foreach (var failed in failedlist)
                {
                    sb.AppendLine(failed);
                }

                using (StreamWriter outfile = new StreamWriter(Program.Config.ResultsDirectory + "\\" + FailedLog, true))
                {
                    try
                    {
                        outfile.WriteAsync(sb.ToString());
                        Logger.Log("Wrote \"Remove TightVNC Failed\" results to file " + Program.Config.ResultsDirectory + "\\" + FailedLog);
                    }
                    catch (Exception e)
                    {
                        Logger.Log("Unable to write to " + FailedLog + ". \n" + e.InnerException);
                        MessageBox.Show("Unable to write to " + FailedLog + ". \n" + e.InnerException);
                    }
                }
            }
        }
    }
}