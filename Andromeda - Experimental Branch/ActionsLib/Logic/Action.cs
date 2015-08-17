using System;
using System.Collections.Generic;
using System.Linq;
using Andromeda.Model;


namespace Andromeda.Logic
{
    public class Action
    {
        public ResultConsole ResultConsole { get { return ResultConsole.Instance; } }
        protected Configuration Config { get { return ConfigManager.CurrentConfig; } }
        protected NetworkConnections netConn;

        public string ActionName { get; protected set; }
        public string Description { get; protected set; }
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

            List<string> resultList = new List<string>();

            foreach (var d in devList)
            {
                var t = d;

                t = new string(t.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray());

                resultList.Add(t);
            }

            ProgressData.OnStartProgressBar(resultList.Count);

            return resultList;
        }

        public bool ValidateCredentials(CredToken credentialToken)
        {
            if (credentialToken == null||
                credentialToken.User == "" || 
                credentialToken.User == "USERNAME" || 
                credentialToken.User == "username")
            {
                return false;
            }

            return true;
        }
    }
}
