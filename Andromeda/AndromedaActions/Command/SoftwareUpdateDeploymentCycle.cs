using AndromedaCore;
using AndromedaCore.Infrastructure;

namespace AndromedaActions.Command
{
    public class SoftwareUpdateDeploymentCycle : SccmScheduleActionBase
    {
        public SoftwareUpdateDeploymentCycle(ILoggerService logger, IWmiServices wmiService,
            ISccmClientServices sccmClientService, INetworkServices networkServices,
            IFileAndFolderServices fileAndFolderServices)
            : base(logger, wmiService, sccmClientService, networkServices, fileAndFolderServices)
        {
            ActionName = "Client Action - Software Update Deployment Cycle";
            Description = "Forces SCCM to schedule a Software Update Deployment check on the client.";
            Category = "SCCM";
        }

        public override void RunCommand(string rawDeviceList)
        {
            RunScheduleTrigger(SoftwareUpdateScanCycleScheduleId, rawDeviceList);
        }
    }
}