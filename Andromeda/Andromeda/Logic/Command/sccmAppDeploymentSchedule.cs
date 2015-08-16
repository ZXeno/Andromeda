using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Andromeda.Command
{
    public class SccmAppDeploymentSchedule : SccmScheduleActionBase
    {

        public SccmAppDeploymentSchedule()
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