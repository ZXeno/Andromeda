using AndromedaCore;
using AndromedaCore.Infrastructure;

namespace AndromedaActions.Command
{
    public class UserPolicyRetrievalCycleSccm : SccmScheduleActionBase
    {

        public UserPolicyRetrievalCycleSccm(ILoggerService logger, IWmiServices wmiService, ISccmClientServices sccmClientService, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices) 
            : base(logger, wmiService, sccmClientService, networkServices, fileAndFolderServices)
        {
            ActionName = "User Policy Retrieval Cycle";
            Description = "Forces SCCM to schedule a User Policy Retrieval check on the client.";
            Category = "SCCM Client Actions";
        }

        public override void RunCommand(string rawDeviceList)
        {
            RunScheduleTrigger(UserPolicyRetrievalCycleScheduleId, rawDeviceList);
        }

    }
}