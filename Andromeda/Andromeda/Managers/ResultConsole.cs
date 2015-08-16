using System;
using System.Collections.Generic;
using System.IO;
using Andromeda.Model;


namespace Andromeda
{
    public class ResultConsole
    {
        public delegate void ConsoleEvent(string updateData);
        public static event ConsoleEvent ConsoleChange;

        private static Configuration Config { get { return ConfigManager.CurrentConfig; } }
        private static ResultConsole _instance;
        public static ResultConsole Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ResultConsole();
                }
                return _instance;
            }
            set
            {
                if (_instance == null || _instance != value)
                {
                    _instance = value;
                }
            }
        }

        private List<string> _history; 
        public List<string> History
        {
            get { return _history; }
            private set
            {
                _history = value;
            }
        }

        private string _consoleString;
        public string ConsoleString
        {
            get { return _consoleString; }
            private set
            {
                _consoleString = value;
                OnConsoleChange(value);
            }
        }

        public bool _isInitialized = false;
        public bool IsInitialized { get { return _isInitialized; } }

        public void OnConsoleChange(string updateData)
        {
            if (ConsoleChange != null)
            {
                ConsoleChange(updateData);
            }
        }

        public ResultConsole()
        {
            _instance = this;
            _history = new List<string>();
            _consoleString = "";
            _isInitialized = true;
        }

        public void AddConsoleLine(string str)
        {
            if (_isInitialized)
            {
                History.Add(str + "\n");
                ConsoleString += str + "\n";

                if (Config != null)
                {
                    if (Config.AlwaysDumpConsoleHistory)
                    {
                        AddLineToHistoryDumpFile(str);
                        Logger.Log("History dump at " + DateTime.Now + ". Contents: " + str);
                    }
                }
            }
        }

        // Dumps the entire console and console history to a log file.
        public void DumpConsoleHistoryToLogFile()
        {
            string historydump = "";
            foreach (var entry in History)
            {
                historydump += entry;
            }

            var filepath = Config.ResultsDirectory + DateTime.Now + "_console_dump.txt";
            WriteToTextFile.WriteToLogFile(filepath, historydump);
        }

        private void AddLineToHistoryDumpFile(string line)
        {
            var filePath = Config.ResultsDirectory + "\\" + DateTime.Today.Month + "_" + DateTime.Today.Day + "_" + DateTime.Today.Year + "_history_dump_file.txt";

            if (!File.Exists(filePath))
            {
                WriteToTextFile.CreateNewLogFile(filePath);
                WriteToTextFile.AddLineToFile(filePath, line);
            }
            else
            {
                WriteToTextFile.AddLineToFile(filePath, line);
            }
        }

        
    }
}