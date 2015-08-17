namespace Andromeda.Logic.Command
{
    public class SoftwareInventoryCycleSccm : SccmScheduleActionBase
    {
        public SoftwareInventoryCycleSccm()
        {
            ActionName = "Software Inventory Cycle";
            Description = "Forces SCCM to schedule a Softweare Inventory check on the client.";
            Category = ActionGroup.SCCM;
        }

        public override void RunCommand(string a)
        {
            RunScheduleTrigger(SccmClientFuncs.SoftwareInventoryCycleScheduleId, a);
        }
    }
}