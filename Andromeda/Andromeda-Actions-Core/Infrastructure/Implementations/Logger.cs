using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;

namespace Andromeda_Actions_Core.Infrastructure
{
    public class Logger : ILoggerService
    {
        private static string _logFilePath;
        private const string LogFileName = "LogFile.txt";
        private const string FallbackLogFileName = "Andromeda_Fallback_Log.log";
        private const string FallbackLogFilePath = "C:\\Temp\\";
        private static string _fullLogPath;

        private const string ErrorString = "[ERROR]";
        private const string WarningString = "[WARNING]";
        private const string MessageString = "[MESSAGE]";
        private const string StackTraceString = "[STACK TRACE]";

        private static readonly Queue<string> Queue = new Queue<string>();
        private static readonly AutoResetEvent _hasNewItems = new AutoResetEvent(false);
        private static volatile bool _waiting = false;
        private static bool _appIsExiting = false;

        private static IFileAndFolderServices _fileAndFolderServices;

        public Logger(IFileAndFolderServices fileAndFolderServices)
        {
            _fileAndFolderServices = fileAndFolderServices;
            Application.Current.Exit += OnApplicationExit;

            _logFilePath = Environment.CurrentDirectory + "\\logs";
            _fullLogPath = _logFilePath + "\\" + LogFileName;

            ValidateLogDirectoryExists();

            if (!File.Exists(_fullLogPath))
            {
                _fileAndFolderServices.CreateNewTextFile(_fullLogPath);
            }

            var loggingThread = new Thread(new ThreadStart(ProcessQueue));
            loggingThread.IsBackground = true;
            loggingThread.Start();

            LogMessage("Logger initiated.");
        }

        public void LogMessage(string msg)
        {
            lock (Queue)
            {
                Queue.Enqueue($"[{DateTime.Now}] {MessageString} {msg}");
            }
            _hasNewItems.Set();
        }

        public void LogWarning(string msg, Exception e)
        {
            lock (Queue)
            {
                Queue.Enqueue($"[{DateTime.Now}] {WarningString} {msg}");

                if (e != null)
                {
                    Queue.Enqueue($"[{DateTime.Now}] -- {WarningString} {e.Message}");
                }
            }
            _hasNewItems.Set();
        }

        public void LogError(string msg, Exception e)
        {
            lock (Queue)
            {
                Queue.Enqueue($"[{DateTime.Now}] {ErrorString} {msg}");
                Queue.Enqueue($"[{DateTime.Now}] {StackTraceString} {e.StackTrace}");
            }
            _hasNewItems.Set();
        }

        private void ProcessQueue()
        {
            while (true)
            {
                if (_appIsExiting) { break; }

                _waiting = true;
                _hasNewItems.WaitOne(10000, true);
                _waiting = false;

                Queue<string> queueCopy;
                lock (Queue)
                {
                    queueCopy = new Queue<string>(Queue);
                    Queue.Clear();
                }

                var sb = new StringBuilder();

                foreach (var line in queueCopy)
                {
                    sb.AppendLine(line);
                }

                try
                {
                    _fileAndFolderServices.WriteToTextFile(_fullLogPath, sb.ToString(), this);
                }
                catch (Exception e)
                {
                    LogFallbackException(e);
                }
                
                queueCopy.Clear();
            }
        }

        public void Flush()
        {
            while (!_waiting)
            {
                Thread.Sleep(1);
            }
        }

        private void LogFallbackException(Exception exception)
        {
            var fullpath = $"{FallbackLogFilePath}{FallbackLogFileName}";

            if (!Directory.Exists(FallbackLogFilePath))
            {
                Directory.CreateDirectory(FallbackLogFilePath);
            }

            if (!File.Exists(fullpath))
            {
                _fileAndFolderServices.CreateNewTextFile(fullpath);
            }

            var sb = new StringBuilder();
            sb.AppendLine($"[{DateTime.Now}]:LOGGER EXCEPTION MESSAGE:{exception.Message}");
            sb.AppendLine($"[{DateTime.Now}]:STACK TRACE: {exception.StackTrace}");

            _fileAndFolderServices.WriteToTextFile(fullpath, sb.ToString(), this);
        }

        private void ValidateLogDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(_logFilePath))
                {
                    Directory.CreateDirectory(_logFilePath);
                }
            }
            catch (Exception ex)
            {
                ResultConsole.Instance.AddConsoleLine("There was an exception when validating or creating the folder used for logging.");
                ResultConsole.Instance.AddConsoleLine(ex.Message);
            }
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            _appIsExiting = true;
        }
    }
}