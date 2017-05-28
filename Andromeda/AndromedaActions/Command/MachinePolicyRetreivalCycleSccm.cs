using AndromedaCore;
using AndromedaCore.Infrastructure;

namespace AndromedaActions.Command
{
    public class MachinePolicyRetreivalCycleSccm : SccmScheduleActionBase
    {
        public MachinePolicyRetreivalCycleSccm(ILoggerService logger, IWmiServices wmiService, ISccmClientServices sccmClientService, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices) 
            : base(logger, wmiService, sccmClientService, networkServices, fileAndFolderServices)
        {
            ActionName = "Client Action - Machine Policy Retreival Cycle";
            Description = "Forces SCCM to schedule a Machine Policy Retreival Cycle on the client.";
            Category = "SCCM";
        }

        public override void RunCommand(string rawDeviceList)
        {
            RunScheduleTrigger(MachinePolicyRetrievalCycleScheduleId, rawDeviceList);
        }

    }
}