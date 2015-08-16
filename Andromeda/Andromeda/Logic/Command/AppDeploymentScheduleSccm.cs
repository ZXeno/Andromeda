namespace Andromeda.Logic.Command
{
    public class AppDeploymentScheduleSccm : SccmScheduleActionBase
    {

        public AppDeploymentScheduleSccm()
        {
            ActionName = "Application Deployment Evaluation Cycle";
            Description = "Forces SCCM to schedule an Application Deployment check on the client.";
            Category = ActionGroup.SCCM;
        }

        public override void RunCommand(string a)
        {
            RunScheduleTrigger(SccmClientFuncs.ApplicationDeploymentEvaluationCycleScheduleId, a);
        }
    }
}