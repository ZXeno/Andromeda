using AndromedaCore;
using AndromedaCore.Infrastructure;

namespace AndromedaActions.Command
{
    public class SoftwareUpdateScanCycle : SccmScheduleActionBase
    {
        public SoftwareUpdateScanCycle(ILoggerService logger, IWmiServices wmiService,
            ISccmClientServices sccmClientService, INetworkServices networkServices,
            IFileAndFolderServices fileAndFolderServices)
            : base(logger, wmiService, sccmClientService, networkServices, fileAndFolderServices)
        {
            ActionName = "Software Update Scan Cycle";
            Description = "Forces SCCM to schedule a Software Update check on the client.";
            Category = "SCCM Client Actions";
        }

        public override void RunCommand(string rawDeviceList)
        {
            RunScheduleTrigger(SoftwareUpdateScanCycleScheduleId, rawDeviceList);
        }
    }
}