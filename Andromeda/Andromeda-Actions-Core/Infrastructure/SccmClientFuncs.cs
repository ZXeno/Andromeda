using System;
using System.Management;
using Andromeda_Actions_Core.Infrastructure;

namespace Andromeda_Actions_Core
{
    public static class SccmClientFuncs
    {
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

        public static void TriggerClientAction(string scheduleId, ManagementScope remote)
        {
            ObjectQuery query = new SelectQuery("SELECT * FROM meta_class WHERE __Class = 'SMS_Client'");
            EnumerationOptions eOption = new EnumerationOptions();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(remote, query, eOption);
            ManagementObjectCollection queryCollection = searcher.Get();

            foreach (ManagementObject ro in queryCollection)
            {
                // Obtain in-parameters for the method
                ManagementBaseObject inParams = ro.GetMethodParameters("TriggerSchedule");

                // Add the input parameters.
                inParams["sScheduleID"] = scheduleId;

                try
                {
                    var outParams = ro.InvokeMethod("TriggerSchedule", inParams, null);

                    ResultConsole.Instance.AddConsoleLine($"Returned with value {WMIFuncs.GetProcessReturnValueText(Convert.ToInt32(outParams["ReturnValue"]))}");
                }
                catch (Exception ex)
                {
                    ResultConsole.Instance.AddConsoleLine("Error performing SCCM Client Function due to an error.");
                    Logger.Log($"Error performing SCCM Client Function due to the following error: {ex.Message}");
                }
            }
        }
    }
}