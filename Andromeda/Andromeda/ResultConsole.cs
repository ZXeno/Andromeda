using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Andromeda
{
    public static class ResultConsole
    {
        public delegate void ConsoleEvent();
        public static event ConsoleEvent ConsoleChange;

        private static List<string> history;
        private static string consoleString = "";

        public static List<string> History { get { return history; } }
        public static string ConsoleString { get { return consoleString; } }
        public static int LoggingLevel { get; set; }

        public static void InitializeResultConsole()
        {
            consoleString = "";
            history = new List<string>();
            AddConsoleLine("Result console started -- " + DateTime.Now.ToString());
        }

        public static void AddConsoleLine(string str)
        {
            History.Add(str + "\n");
            consoleString += str + "\n";
            OnConsoleEvent();
        }

        public static void OnConsoleEvent()
        {
            if (ConsoleChange != null)
            {
                ConsoleChange();
            }
        }

        // Dumps the entire console and console history to a log file.
        public static void DumpConsoleHistoryToLogFile()
        {
            string historydump = "";
            foreach (string entry in History)
            {
                historydump += entry + "\n";
            }

            string filepath = Environment.CurrentDirectory + "\\results\\" + DateTime.Now + ".txt";
            WriteToTextFile.WriteToLogFile(filepath, historydump);
        }
    }
}
