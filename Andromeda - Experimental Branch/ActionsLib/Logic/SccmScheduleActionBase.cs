using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Text;
using System.Windows;
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
                ResultConsole.AddConsoleLine("There were " + failedlist.Count + "computers that failed the process. They have been recorded in the log.");
                StringBuilder sb = new StringBuilder();

                if (File.Exists(Config.ResultsDirectory + "\\" + FailedLog))
                {
                    File.Delete(Config.ResultsDirectory + "\\" + FailedLog);
                    Logger.Log("Deleted file " + Config.ResultsDirectory + "\\" + FailedLog);
                }

                foreach (var failed in failedlist)
                {
                    sb.AppendLine(failed);
                }

                using (StreamWriter outfile = new StreamWriter(Config.ResultsDirectory + "\\" + FailedLog, true))
                {
                    try
                    {
                        outfile.WriteAsync(sb.ToString());
                        Logger.Log("Wrote \"Remove TightVNC Failed\" results to file " + Config.ResultsDirectory + "\\" + FailedLog);
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