using System;
using System.Collections.Generic;
using System.Linq;
using Andromeda.Infrastructure;
using Andromeda.Model;

namespace Andromeda.Logic
{
    public abstract class Action
    {
        public ResultConsole ResultConsole { get { return ResultConsole.Instance; } }
        protected Configuration Config { get { return ConfigManager.CurrentConfig; } }
        protected NetworkConnections netConn;

        public string ActionName { get; protected set; }
        public string Description { get; protected set; }
        public ActionGroup Category { get; protected set; }

        // Single entry
        public abstract void RunCommand(string rawDeviceList);

        // Used for returning the name of the command to the GUI
        public override string ToString() { return ActionName; }

        // Return a list of devices from the string list of the GUI
        public List<string> ParseDeviceList(string list)
        {
            List<string> devList = new List<string>(list.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));

            return devList.Select(t => new string(t.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray())).ToList();
        }
    }
}
