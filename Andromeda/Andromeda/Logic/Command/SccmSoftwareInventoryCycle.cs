using System.Threading.Tasks;

namespace Andromeda.Command
{
    public class SccmSoftwareInventoryCycle : SccmScheduleActionBase
    {
        public SccmSoftwareInventoryCycle()
        {
            ActionName = "Software Inventory Cycle";
            Description = "Forces SCCM to schedule a Softweare Inventory check on the client.";
            Category = ActionGroup.SCCM;
        }

        public override void RunCommand(string a)
        {
            RunScheduleTrigger(SccmClientFuncs.SoftwareInventoryCycleScheduleId, a);
        }
    }
}