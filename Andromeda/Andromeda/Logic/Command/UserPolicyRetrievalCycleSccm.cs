namespace Andromeda.Logic.Command
{
    public class UserPolicyRetrievalCycleSccm : SccmScheduleActionBase
    {

        public UserPolicyRetrievalCycleSccm()
        {
            ActionName = "User Policy Retrieval Cycle";
            Description = "Forces SCCM to schedule a User Policy Retrieval check on the client.";
            Category = ActionGroup.SCCM;
        }

        public override void RunCommand(string a)
        {
            RunScheduleTrigger(SccmClientFuncs.UserPolicyRetrievalCycleScheduleId, a);
        }

    }
}