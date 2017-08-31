using AndromedaCore;
using AndromedaCore.Infrastructure;

namespace AndromedaActions.Command
{
    public class AppDeploymentScheduleSccm : SccmScheduleActionBase
    {

        public AppDeploymentScheduleSccm(ILoggerService logger, IWmiServices wmiService, ISccmClientServices sccmClientService, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices) 
            : base(logger, wmiService, sccmClientService, networkServices, fileAndFolderServices)
        {
            ActionName = "Application Deployment Evaluation Cycle";
            Description = "Forces SCCM to schedule an Application Deployment check on the client.";
            Category = "SCCM Client Actions";
        }

        public override void RunCommand(string rawDeviceList)
        {
            RunScheduleTrigger(ApplicationDeploymentEvaluationCycleScheduleId, rawDeviceList);
        }
    }
}