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

        private static Configuration Config => ConfigManager.CurrentConfig;
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

        public ResultConsole()
        {
            _instance = this;
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
                    ConsoleString += str + "\n";
                }

                queueCopy.Clear();
            }
        }
    }
}