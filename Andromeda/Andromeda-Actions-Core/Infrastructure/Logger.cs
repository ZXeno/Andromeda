using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Andromeda_Actions_Core.Infrastructure
{
    public class Logger
    {
        private static string _logFilePath;
        private const string LogFileName = "LogFile.txt";
        private const string FallbackLogFileName = "Andromeda_Fallback_Log.log";
        private const string FallbackLogFilePath = "C:\\Temp\\";
        private static string _fullLogPath;

        private static readonly Queue<string> Queue = new Queue<string>();
        private static readonly AutoResetEvent _hasNewItems = new AutoResetEvent(false);
        private static volatile bool _waiting = false;

        private static IFileAndFolderServices _fileAndFolderServices;

        public Logger(string userFolder, IFileAndFolderServices fileAndFolderServices)
        {
            _fileAndFolderServices = fileAndFolderServices;

            _logFilePath = userFolder + "\\logs";
            _fullLogPath = _logFilePath + "\\" + LogFileName;

            ValidateLogDirectoryExists();

            if (!File.Exists(_fullLogPath))
            {
                _fileAndFolderServices.CreateNewTextFile(_fullLogPath);
            }

            var loggingThread = new Thread(new ThreadStart(ProcessQueue));
            loggingThread.IsBackground = true;
            loggingThread.Start();

            Log("Logger initiated.");
        }

        public static void Log(string line)
        {
            lock (Queue)
            {
                Queue.Enqueue($"{DateTime.Now} {line}");
            }
            _hasNewItems.Set();
        }

        private void ProcessQueue()
        {
            while (true)
            {
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
                    _fileAndFolderServices.WriteToTextFile(_fullLogPath, sb.ToString());
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

            _fileAndFolderServices.WriteToTextFile(fullpath, sb.ToString());
        }

        private static void ValidateLogDirectoryExists()
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

    }
}