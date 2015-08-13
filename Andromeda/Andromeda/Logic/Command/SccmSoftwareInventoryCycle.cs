namespace Andromeda.Command
{
    public class SccmSoftwareInventoryCycle : SccmScheduleActionBase
    {
        public SccmSoftwareInventoryCycle()
        {
            ActionName = "Software Inventory Cycle";
            Description = "Forces SCCM to schedule a Softweare Inventory check on the client.";
            Category = ActionGroup.SCCM;
        }

        public override void RunCommand(string deviceList)
        {
            RunScheduleTrigger(SccmClientFuncs.SoftwareInventoryCycleScheduleId, deviceList);
        }
    }
}