using AndromedaCore;
using AndromedaCore.Infrastructure;

namespace AndromedaActions.Command
{
    public class SoftwareMeteringUsageReportCycle : SccmScheduleActionBase
    {
        public SoftwareMeteringUsageReportCycle(ILoggerService logger, IWmiServices wmiService,
            ISccmClientServices sccmClientService, INetworkServices networkServices,
            IFileAndFolderServices fileAndFolderServices)
            : base(logger, wmiService, sccmClientService, networkServices, fileAndFolderServices)
        {
            ActionName = "Client Action - Software Metering Usage Report Cycle";
            Description = "Forces SCCM to schedule a Software Metering Usage Report check on the client.";
            Category = "SCCM";
        }

        public override void RunCommand(string rawDeviceList)
        {
            RunScheduleTrigger(SoftwareMeteringUsageReportCycleScheduleId, rawDeviceList);
        }
    }
}