using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Text;
using System.Windows;
using Andromeda.Model;
using Andromeda.ViewModel;

namespace Andromeda.Command
{
    public class AppDeploymentSchedule : Action
    {
        private ConnectionOptions _connOps;
        private string _failedFile = "sccm_appdeploymentschedule_faile_log.txt";
        //private CredToken _creds;

        public AppDeploymentSchedule()
        {
            ActionName = "Application Deployment Evaluation Cycle";
            Description = "Forces SCCM to schedule an Application Deployment check on the client.";
            Category = ActionGroup.SCCM;
            _connOps = new ConnectionOptions();
        }

        public override void RunCommand(string deviceList)
        {
            List<string> devlist = ParseDeviceList(deviceList);
            List<string> successList = GetPingableDevices.GetDevices(devlist);
            List<string> failedlist = new List<string>();

            string scope = "\\root\\ccm";
            _connOps.EnablePrivileges = true;

            foreach (var d in successList)
            {
                var remote = WMIFuncs.ConnectToRemoteWMI(d, scope, _connOps);
                if (remote != null)
                {
                    ObjectQuery query = new SelectQuery("SMS_Client");

                    ManagementObjectSearcher searcher = new ManagementObjectSearcher(remote, query);
                    ManagementObjectCollection queryCollection = searcher.Get();

                    foreach (var resultobject in queryCollection)
                    {
                        ManagementObject ro = resultobject as ManagementObject;
                        // Obtain in-parameters for the method
                        ManagementBaseObject inParams = ro.GetMethodParameters("TriggerSchedule");

                        // Add the input parameters.
                        inParams["sScheduleID"] = "{00000000-0000-0000-0000-000000000021}";

                        try
                        {
                            ManagementBaseObject outParams = ro.InvokeMethod("TriggerSchedule", inParams, null);

                            ResultConsole.AddConsoleLine("Returned with value " + WMIFuncs.GetProcessReturnValueText(Convert.ToInt32(outParams["ReturnValue"])));
                        }
                        catch (ManagementException ex)
                        {
                            MessageBox.Show(ex.HResult.ToString());
                        }



                    }

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

                if (File.Exists(Program.Config.ResultsDirectory + "\\" + _failedFile))
                {
                    File.Delete(Program.Config.ResultsDirectory + "\\" + _failedFile);
                    Logger.Log("Deleted file " + Program.Config.ResultsDirectory + "\\" + _failedFile);
                }

                foreach (var failed in failedlist)
                {
                    sb.AppendLine(failed);
                }

                using (StreamWriter outfile = new StreamWriter(Program.Config.ResultsDirectory + "\\" + _failedFile, true))
                {
                    try
                    {
                        outfile.WriteAsync(sb.ToString());
                        Logger.Log("Wrote \"Remove TightVNC Failed\" results to file " + Program.Config.ResultsDirectory + "\\" + _failedFile);
                    }
                    catch (Exception e)
                    {
                        Logger.Log("Unable to write to " + _failedFile + ". \n" + e.InnerException);
                        MessageBox.Show("Unable to write to " + _failedFile + ". \n" + e.InnerException);
                    }
                }
            }

        }
    }
}