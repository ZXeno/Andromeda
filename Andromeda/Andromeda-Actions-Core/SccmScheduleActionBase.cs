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

        public const string ApplicationDeploymentEvaluationCycleScheduleId = "{00000000-0000-0000-0000-000000000121}";
        public const string DiscoveryDataCollectionCycleScheduleId = "{00000000-0000-0000-0000-000000000003}";
        public const string FileCollectionCycleScheduleId = "{00000000-0000-0000-0000-000000000010}";
        public const string HardwareInventoryCycleScheduleId = "{00000000-0000-0000-0000-000000000001}";
        public const string MachinePolicyRetrievalCycleScheduleId = "{00000000-0000-0000-0000-000000000021}";
        public const string MachinePolicyEvaluationCycleScheduleId = "{00000000-0000-0000-0000-000000000022}";
        public const string SoftwareInventoryCycleScheduleId = "{00000000-0000-0000-0000-000000000002}";
        public const string SoftwareMeteringUsageReportCycleScheduleId = "{00000000-0000-0000-0000-000000000031}";
        public const string SoftwareUpdateDeploymentEvaluationCycleScheduleId = "{00000000-0000-0000-0000-000000000114}";
        public const string SoftwareUpdateScanCycleScheduleId = "{00000000-0000-0000-0000-000000000113}";
        public const string StateMessageRefreshScheduleId = "{00000000-0000-0000-0000-000000000111}";
        public const string UserPolicyRetrievalCycleScheduleId = "{00000000-0000-0000-0000-000000000026}";
        public const string UserPolicyEvaluationCycleScheduleId = "{00000000-0000-0000-0000-000000000027}";
        public const string WindowsInstallersSourceListUpdateCycleScheduleId = "{00000000-0000-0000-0000-000000000032}";

        protected readonly IWmiServices WmiService;
        protected readonly ISccmClientServices SccmClientService;

        protected SccmScheduleActionBase(IWmiServices wmiService, ISccmClientServices sccmClientService, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices) : base(networkServices, fileAndFolderServices)
        {
            WmiService = wmiService;
            SccmClientService = sccmClientService;
        }

        protected virtual void RunScheduleTrigger(string scheduleId, string deviceList)
        {
            var devlist = ParseDeviceList(deviceList);
            var failedlist = new List<string>();
            Connection = new ConnectionOptions { EnablePrivileges = true };

            try
            {
                Parallel.ForEach(devlist, (device) =>
                {
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    if (!NetworkServices.VerifyDeviceConnectivity(device))
                    {
                        failedlist.Add(device);
                        ResultConsole.Instance.AddConsoleLine($"Device {device} failed connection verification. Added to failed list.");
                        return;
                    }

                    ManagementScope remote = null;
                    var remoteConnectExceptionMsg = "";

                    try
                    {
                        remote = WmiService.ConnectToRemoteWmi(device, Scope, Connection);
                    }
                    catch (Exception ex)
                    {
                        remoteConnectExceptionMsg = ex.Message;
                    }

                    if (remote != null)
                    {
                        SccmClientService.TriggerClientAction(scheduleId, remote);
                    }
                    else
                    {
                        ResultConsole.AddConsoleLine($"Error connecting to WMI scope {device}. Process aborted for this device.");
                        Logger.Log($"Error connecting to WMI scope {device}. Process aborted for this device. Exception message: {remoteConnectExceptionMsg}");
                        failedlist.Add(device);
                    }
                });
            }
            catch (OperationCanceledException e)
            {
                ResultConsole.AddConsoleLine($"Operation {ActionName} canceled.");
                Logger.Log($"Operation {ActionName} canceled by user request. {e.Message}");
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