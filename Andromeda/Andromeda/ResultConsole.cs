using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Andromeda
{
    public class ResultConsole
    {
        public delegate void ConsoleEvent();
        public static event ConsoleEvent ConsoleChange;

        private List<string> resultconents;
        private string consolestring = "NULL";

        public List<string> Contents 
        {
            get { return resultconents; }
        }

        public string ConsoleString 
        { 
            get { return consolestring; } 
        }

        public ResultConsole()
        {
            consolestring = "";
            resultconents = new List<string>();
            AddConsoleLine("Result console started -- " + System.DateTime.Now.ToString());
        }

        public void AddConsoleLine(string str)
        {
            resultconents.Add(str + "\n");
            consolestring += str + "\n";
            OnConsoleEvent();
        }

        public static void OnConsoleEvent()
        {
            if (ConsoleChange != null)
            {
                ConsoleChange();
            }
        }
    }
}
