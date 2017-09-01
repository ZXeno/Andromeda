using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AndromedaCore.Managers;

namespace AndromedaCore.Infrastructure
{
    public class Logger : ILoggerService, IDisposable
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

        private readonly Queue<string> _queue = new Queue<string>();
        
        private readonly Task _loggingTask;
        private readonly AutoResetEvent _hasNewItems = new AutoResetEvent(false);
        private const int WaitPeriod = 5000;
        private readonly CancellationTokenSource _cancellationToken; 

        private static IFileAndFolderServices _fileAndFolderServices;

        public Logger(IFileAndFolderServices fileAndFolderServices)
        {
            _fileAndFolderServices = fileAndFolderServices;
            Application.Current.Exit += OnApplicationExit;
            
            _logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Andromeda\\Logs";
            _fullLogPath = _logFilePath + "\\" + LogFileName;

            ValidateLogDirectoryExists();

            if (!File.Exists(_fullLogPath))
            {
                _fileAndFolderServices.CreateNewTextFile(_fullLogPath);
            }

            _cancellationToken = new CancellationTokenSource();
            _loggingTask = Task.Factory.StartNew(ProcessQueue, _cancellationToken.Token);

            LogMessage("Logger initiated.");
        }

        public void LogMessage(string msg)
        {
            lock (_queue)
            {
                _queue.Enqueue($"[{DateTime.Now}] {MessageString} {msg}");
            }
            _hasNewItems.Set();
        }

        public void LogWarning(string msg, Exception e)
        {
            lock (_queue)
            {
                _queue.Enqueue($"[{DateTime.Now}] {WarningString} {msg}");

                if (e != null)
                {
                    _queue.Enqueue($"[{DateTime.Now}] -- {WarningString} {e.Message}");
                }
            }
            _hasNewItems.Set();
        }

        public void LogError(string msg, Exception e)
        {
            lock (_queue)
            {
                _queue.Enqueue($"[{DateTime.Now}] {ErrorString} {msg}| ERROR_MESSAGE:{e.Message}");
                _queue.Enqueue($"[{DateTime.Now}] {StackTraceString} {e.StackTrace}");
            }
            _hasNewItems.Set();
        }

        private void ProcessQueue()
        {
            while (true)
            {
                if (_cancellationToken.IsCancellationRequested) { break; }
                
                _hasNewItems.WaitOne(WaitPeriod);

                Queue<string> queueCopy;
                lock (_queue)
                {
                    queueCopy = new Queue<string>(_queue);
                    _queue.Clear();
                }

                var sb = new StringBuilder();

                foreach (var line in queueCopy)
                {
                    sb.AppendLine(line);
                }

                try
                {
                    if (!string.IsNullOrWhiteSpace(sb.ToString()))
                    {
                        _fileAndFolderServices.WriteToTextFile(_fullLogPath, sb.ToString(), this);
                    }
                }
                catch (Exception e)
                {
                    LogFallbackException(e);
                }
                
                queueCopy.Clear();
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
            _cancellationToken.Cancel();
        }

        public void Dispose()
        {
            _loggingTask?.Dispose();
            _cancellationToken?.Dispose();
        }
    }
}