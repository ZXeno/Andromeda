using System;
using System.Management;

namespace Andromeda_Actions_Core.Infrastructure
{
    public class SccmClientServices : ISccmClientServices
    {
        private readonly IWmiServices _wmiServices;
        private readonly ILoggerService _logger;

        public SccmClientServices(ILoggerService logger, IWmiServices wmiServices)
        {
            _logger = logger;
            _wmiServices = wmiServices;
        }

        public void TriggerClientAction(string scheduleId, ManagementScope remote)
        {
            ObjectQuery query = new SelectQuery("SELECT * FROM meta_class WHERE __Class = 'SMS_Client'");
            var eOption = new EnumerationOptions();
            var searcher = new ManagementObjectSearcher(remote, query, eOption);
            var queryCollection = searcher.Get();

            foreach (ManagementObject ro in queryCollection)
            {
                // Obtain in-parameters for the method
                var inParams = ro.GetMethodParameters("TriggerSchedule");

                // Add the input parameters.
                inParams["sScheduleID"] = scheduleId;

                try
                {
                    var outParams = ro.InvokeMethod("TriggerSchedule", inParams, null);

                    ResultConsole.Instance.AddConsoleLine($"Returned with value {_wmiServices.GetProcessReturnValueText(Convert.ToInt32(outParams["ReturnValue"]))}");
                }
                catch (Exception ex)
                {
                    ResultConsole.Instance.AddConsoleLine("Error performing SCCM Client Function due to an error.");
                    _logger.LogError($"Error performing SCCM Client Function due to the following error: {ex.Message}", ex);
                }
            }
        }
    }
}