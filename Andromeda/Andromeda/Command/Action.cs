using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum ActionGroup { Schedule, Agent, Health, Advertisement, Other }

namespace Andromeda.Command
{
    public class Action
    {
        public string ActionName { get; protected set; }
        public string Desctiption { get; protected set; }
        public ActionGroup Category { get; protected set; }

        public Action() { }

        // Single entry
        public virtual string RunCommand(string a) { return null; }

        // These are intended to return results tied to device keys.
        public virtual Dictionary<string,string> RunCommand(string[] a) 
        {
            return null;
        }
        public virtual Dictionary<string,string> RunCommand(List<string> a) 
        {
            return null;
        }

    }
}
