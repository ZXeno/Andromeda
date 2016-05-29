using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Andromeda_Actions_Core.Infrastructure;
using Andromeda_Actions_Core.Model;

namespace Andromeda_Actions_Core
{
    public class ResultConsole
    {
        public delegate void ConsoleEvent(string updateData);
        public static event ConsoleEvent ConsoleChange;

        private static Configuration Config { get { return ConfigManager.CurrentConfig; } }
        private static ResultConsole _instance;
        public static ResultConsole Instance
        {
            get { return _instance; }
            set
            {
                if (_instance == null || _instance != value)
                {
                    _instance = value;
                }
            }
        }

        public List<string> History { get; }

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

        public bool IsInitialized { get; }

        public void OnConsoleChange(string updateData)
        {
            ConsoleChange?.Invoke(updateData);
        }

        private readonly Queue<string> _queue = new Queue<string>();
        private readonly AutoResetEvent _hasNewItems = new AutoResetEvent(false);
        private readonly IFileAndFolderServices _fileAndFolderServices;

        public ResultConsole(IFileAndFolderServices fileAndFolderServices)
        {
            _fileAndFolderServices = fileAndFolderServices;

            _instance = this;
            History = new List<string>();
            _consoleString = "";
            IsInitialized = true;

            var loggingThread = new Thread(new ThreadStart(ProcessQueue));
            loggingThread.IsBackground = true;
            loggingThread.Start();
        }

        public void AddConsoleLine(string str)
        {
            if (IsInitialized)
            {
                lock (_queue)
                {
                    _queue.Enqueue(str);
                }
                _hasNewItems.Set();
            }
        }

        private void ProcessQueue()
        {
            while (true)
            {
                _hasNewItems.WaitOne(100, true);

                Queue<string> queueCopy;
                lock (_queue)
                {
                    queueCopy = new Queue<string>(_queue);
                    _queue.Clear();
                }

                foreach (var str in queueCopy)
                {
                    History.Add(str + "\n");
                    ConsoleString += str + "\n";

                    if (Config != null)
                    {
                        if (Config.AlwaysDumpConsoleHistory)
                        {
                            AddLineToHistoryDumpFile(str);
                            Logger.Log($"History dump at {DateTime.Now}. Contents: {str}");
                        }
                    }
                }

                queueCopy.Clear();
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

            var filepath = $"{Config.ResultsDirectory}{DateTime.Now}_console_dump.txt";
            _fileAndFolderServices.WriteToTextFile(filepath, historydump);
        }

        private void AddLineToHistoryDumpFile(string line)
        {
            var filePath = $"{Config.ResultsDirectory}\\_history_dump_file.txt";

            if (!File.Exists(filePath))
            {
                _fileAndFolderServices.CreateNewTextFile(filePath);
            }

            _fileAndFolderServices.WriteToTextFile(filePath, line);
        }
    }
}