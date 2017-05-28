using AndromedaCore;
using AndromedaCore.Infrastructure;

namespace AndromedaActions.Command
{
    public class SoftwareInventoryCycleSccm : SccmScheduleActionBase
    {
        public SoftwareInventoryCycleSccm(ILoggerService logger, IWmiServices wmiService, ISccmClientServices sccmClientService, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices) 
            : base(logger, wmiService, sccmClientService, networkServices, fileAndFolderServices)
        {
            ActionName = "Client Action - Software Inventory Cycle";
            Description = "Forces SCCM to schedule a Softweare Inventory check on the client.";
            Category = "SCCM";
        }

        public override void RunCommand(string rawDeviceList)
        {
            RunScheduleTrigger(SoftwareInventoryCycleScheduleId, rawDeviceList);
        }
    }
}