using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andromeda.Command
{
    class DebugToConsole : Action
    {
        public DebugToConsole(string name, string descriptor, ActionGroup cat)
        {
            ActionName = name;
            Desctiption = descriptor;
            Category = cat;
        }

        public override string RunCommand(string input)
        {
            string sendback = "";

            foreach (string d in ParseDeviceList(input))
            {
                sendback += d + " was modified" + "\n";
            }

            return sendback;
        }
    }
}
