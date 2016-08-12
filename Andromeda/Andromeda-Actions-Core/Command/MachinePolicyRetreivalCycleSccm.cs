using Andromeda_Actions_Core.Infrastructure;

namespace Andromeda_Actions_Core.Command
{
    public class MachinePolicyRetreivalCycleSccm : SccmScheduleActionBase
    {
        public MachinePolicyRetreivalCycleSccm(ILoggerService logger, IWmiServices wmiService, ISccmClientServices sccmClientService, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices) 
            : base(logger, wmiService, sccmClientService, networkServices, fileAndFolderServices)
        {
            ActionName = "Machine Policy Retreival Cycle";
            Description = "Forces SCCM to schedule a Machine Policy Retreival Cycle on the client.";
            Category = ActionGroup.SCCM;
        }

        public override void RunCommand(string rawDeviceList)
        {
            RunScheduleTrigger(MachinePolicyRetrievalCycleScheduleId, rawDeviceList);
        }

    }
}