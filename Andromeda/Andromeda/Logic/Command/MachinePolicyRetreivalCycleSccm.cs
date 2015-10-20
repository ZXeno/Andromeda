namespace Andromeda.Logic.Command
{
    public class MachinePolicyRetreivalCycleSccm : SccmScheduleActionBase
    {
        public MachinePolicyRetreivalCycleSccm()
        {
            ActionName = "Machine Policy Retreival Cycle";
            Description = "Forces SCCM to schedule a Machine Policy Retreival Cycle on the client.";
            Category = ActionGroup.SCCM;
        }

        public override void RunCommand(string rawDeviceList)
        {
            RunScheduleTrigger(SccmClientFuncs.MachinePolicyRetrievalCycleScheduleId, rawDeviceList);
        }

    }
}