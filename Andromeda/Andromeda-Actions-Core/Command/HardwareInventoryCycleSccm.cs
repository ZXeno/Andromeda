namespace Andromeda_Actions_Core.Command
{
    public class HardwareInventoryCycleSccm : SccmScheduleActionBase
    {
         public HardwareInventoryCycleSccm()
        {
            ActionName = "Hardware Inventory Cycle";
            Description = "Forces SCCM to schedule a Hardware Inventory check on the client.";
            Category = ActionGroup.SCCM;
        }

        public override void RunCommand(string rawDeviceList)
        {
            RunScheduleTrigger(SccmClientFuncs.HardwareInventoryCycleScheduleId, rawDeviceList);
        }
    }
}