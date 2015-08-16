using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Andromeda.ViewModel;

namespace Andromeda.Command
{
    public class SccmHardwareInventoryCycle : SccmScheduleActionBase
    {
         public SccmHardwareInventoryCycle()
        {
            ActionName = "Hardware Inventory Cycle";
            Description = "Forces SCCM to schedule a Hardware Inventory check on the client.";
            Category = ActionGroup.SCCM;
        }

        public override void RunCommand(string a)
        {
            RunScheduleTrigger(SccmClientFuncs.HardwareInventoryCycleScheduleId, a);
        }
    }
}