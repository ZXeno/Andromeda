using AndromedaCore;
using AndromedaCore.Infrastructure;

namespace AndromedaActions.Command
{
    public class HardwareInventoryCycleSccm : SccmScheduleActionBase
    {
         public HardwareInventoryCycleSccm(ILoggerService logger, IWmiServices wmiService, ISccmClientServices sccmClientService, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices) 
            : base(logger, wmiService, sccmClientService, networkServices, fileAndFolderServices)
        {
            ActionName = "Hardware Inventory Cycle";
            Description = "Forces SCCM to schedule a Hardware Inventory check on the client.";
            Category = ActionGroup.SCCM;
        }

        public override void RunCommand(string rawDeviceList)
        {
            RunScheduleTrigger(HardwareInventoryCycleScheduleId, rawDeviceList);
        }
    }
}