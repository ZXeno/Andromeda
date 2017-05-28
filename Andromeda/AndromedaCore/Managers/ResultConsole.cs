using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace AndromedaCore.Managers
{
    public class ResultConsole
    {
        public delegate void ConsoleEvent(string updateData);
        public static event ConsoleEvent ConsoleChange;
        
        private static ResultConsole _instance;
        public static ResultConsole Instance
        {
            get => _instance;
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
            get => _consoleString;
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

        private static bool _appIsExiting = false;

        private readonly Queue<string> _queue = new Queue<string>();
        private readonly AutoResetEvent _hasNewItems = new AutoResetEvent(false);
        private static Thread _loggingThread;
        private static int _threadTimeout = 5000;

        public ResultConsole()
        {
            Application.Current.Exit += OnApplicationExit;

            _instance = this;
            _consoleString = "";
            IsInitialized = true;

            _loggingThread = new Thread(new ThreadStart(ProcessQueue)) { IsBackground = true };
            _loggingThread.Start();
        }

        public void AddConsoleLine(string str)
        {
            if (!IsInitialized) { return; }

            lock (_queue)
            {
                _queue.Enqueue(str);
            }

            _hasNewItems.Set();
        }

        private void ProcessQueue()
        {
            while (true)
            {
                if (_appIsExiting) { break; }

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

            if (_appIsExiting)
            {
                _queue.Clear();
                _loggingThread.Join(_threadTimeout);
            }
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            _appIsExiting = true;
        }
    }
}