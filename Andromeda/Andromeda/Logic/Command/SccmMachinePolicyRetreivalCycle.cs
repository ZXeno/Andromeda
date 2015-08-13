namespace Andromeda.Command
{
    public class SccmMachinePolicyRetreivalCycle : SccmScheduleActionBase
    {
        public SccmMachinePolicyRetreivalCycle()
        {
            ActionName = "Machine Policy Retreival Cycle";
            Description = "Forces SCCM to schedule a Machine Policy Retreival Cycle on the client.";
            Category = ActionGroup.SCCM;
        }

        public override void RunCommand(string deviceList)
        {
            RunScheduleTrigger(SccmClientFuncs.MachinePolicyRetrievalCycleScheduleId, deviceList);
        }

    }
}