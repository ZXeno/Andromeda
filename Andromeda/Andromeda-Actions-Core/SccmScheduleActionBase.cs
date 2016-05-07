using System;
using System.Collections.Generic;
using System.Management;
using System.Threading.Tasks;
using Andromeda_Actions_Core.Infrastructure;

namespace Andromeda_Actions_Core
{
    public abstract class SccmScheduleActionBase : Action
    {
        protected ConnectionOptions Connection;
        protected string FailedLog = "sccm_schedule_failed_log.txt";
        protected string Scope = "\\root\\ccm:SMS_Client";

        public virtual void RunScheduleTrigger(string scheduleId, string deviceList)
        {
            List<string> devlist = ParseDeviceList(deviceList);
            List<string> failedlist = new List<string>();
            Connection = new ConnectionOptions { EnablePrivileges = true };

            try
            {
                Parallel.ForEach(devlist, (device) =>
                {
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    if (!VerifyDeviceConnectivity(device))
                    {
                        failedlist.Add(device);
                        ResultConsole.Instance.AddConsoleLine("Device " + device + " failed connection verification. Added to failed list.");
                        return;
                    }

                    ManagementScope remote = null;
                    string remoteConnectExceptionMsg = "";

                    try
                    {
                        remote = WMIFuncs.ConnectToRemoteWMI(device, Scope, Connection);
                    }
                    catch (Exception ex)
                    {
                        remoteConnectExceptionMsg = ex.Message;
                    }

                    if (remote != null)
                    {
                        SccmClientFuncs.TriggerClientAction(scheduleId, remote);
                    }
                    else
                    {
                        ResultConsole.AddConsoleLine("Error connecting to WMI scope " + device + ". Process aborted for this device.");
                        Logger.Log("Error connecting to WMI scope " + device + ". Process aborted for this device. Exception message: " + remoteConnectExceptionMsg);
                        failedlist.Add(device);
                    }
                });
            }
            catch (OperationCanceledException e)
            {
                ResultConsole.AddConsoleLine("Operation " + ActionName + " canceled.");
                Logger.Log("Operation " + ActionName + " canceled by user request. " + e.Message);
                ResetCancelToken();
            }
            catch (Exception) { }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}