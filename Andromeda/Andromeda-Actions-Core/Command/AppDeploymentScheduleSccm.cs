using Andromeda_Actions_Core.Infrastructure;

namespace Andromeda_Actions_Core.Command
{
    public class AppDeploymentScheduleSccm : SccmScheduleActionBase
    {

        public AppDeploymentScheduleSccm(IWmiServices wmiService, ISccmClientServices sccmClientService, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices) : base(wmiService, sccmClientService, networkServices, fileAndFolderServices)
        {
            ActionName = "Application Deployment Evaluation Cycle";
            Description = "Forces SCCM to schedule an Application Deployment check on the client.";
            Category = ActionGroup.SCCM;
        }

        public override void RunCommand(string rawDeviceList)
        {
            RunScheduleTrigger(ApplicationDeploymentEvaluationCycleScheduleId, rawDeviceList);
        }
    }
}