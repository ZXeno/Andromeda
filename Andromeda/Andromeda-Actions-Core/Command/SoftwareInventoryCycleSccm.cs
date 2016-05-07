namespace Andromeda_Actions_Core.Command
{
    public class SoftwareInventoryCycleSccm : SccmScheduleActionBase
    {
        public SoftwareInventoryCycleSccm()
        {
            ActionName = "Software Inventory Cycle";
            Description = "Forces SCCM to schedule a Softweare Inventory check on the client.";
            Category = ActionGroup.SCCM;
        }

        public override void RunCommand(string rawDeviceList)
        {
            RunScheduleTrigger(SccmClientFuncs.SoftwareInventoryCycleScheduleId, rawDeviceList);
        }
    }
}