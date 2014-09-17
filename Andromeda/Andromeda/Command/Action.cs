using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum ActionGroup { SCCM, Other, Debug }

namespace Andromeda.Command
{
    public class Action
    {
        public string ActionName { get; protected set; }
        public string Desctiption { get; protected set; }
        public ActionGroup Category { get; protected set; }

        public Action() { }

        // Single entry
        public virtual void RunCommand(string a) {  }

        // Return results tied to device keys.
        public virtual Dictionary<string, string> RunDictionaryResultCommand(string a) { return null; }
        public virtual Dictionary<string, string> RunDictionaryResultCommand(List<string> a) { return null; }

        // Used for returning the name of the command to the GUI
        public override string ToString() { return ActionName; }

        // Return a list of devices from the string list of the GUI
        public List<string> ParseDeviceList(string list)
        {
            List<string> devList = new List<string>(list.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));

            return devList;
        }
    }
}
