﻿namespace Andromeda.Command
{
    public class SccmUserPolicyRetrievalCycle : SccmScheduleActionBase
    {

        public SccmUserPolicyRetrievalCycle()
        {
            ActionName = "User Policy Retrieval Cycle";
            Description = "Forces SCCM to schedule a User Policy Retrieval check on the client.";
            Category = ActionGroup.SCCM;
        }

        public override void RunCommand(string deviceList)
        {
            RunScheduleTrigger(SccmClientFuncs.UserPolicyRetrievalCycleScheduleId, deviceList);
        }

    }
}