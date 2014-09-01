using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Andromeda
{
    class ResultConsole
    {
        private List<string> resultconents;
        private string consolestring = "";

        public List<string> Contents 
        {
            get { return resultconents; }
            set { resultconents = value; }
        }

        public string ConsoleString { get { return consolestring; } }

        public ResultConsole()
        {
            resultconents = new List<string>();
            resultconents.Add("Result console started -- " + System.DateTime.Now.ToString());
            consolestring = "Result console started -- " + System.DateTime.Now.ToString();
        }

        public void AddConsoleLine(string str)
        {
            Contents.Add(str + "\n");
            consolestring += str + "\n";
        }
    }
}
