using System;
using System.IO;
using Andromeda.ViewModel;

namespace Andromeda
{
    public class Logger
    {
        private static string _logFilePath;
        private static string _logFileName;
        private static string _fullLogPath;

        public Logger()
        {
            _logFilePath = Program.UserFolder + "\\logs";
            _logFileName = DateTime.Today.Month + "_" + DateTime.Today.Day + "_" + DateTime.Today.Year + "_log.txt";

            _fullLogPath = _logFilePath + "\\" + _logFileName;

            ValidateLogDirectoryExists();

            if (!File.Exists(_fullLogPath))
            {
                WriteToTextFile.CreateNewLogFile(_fullLogPath);
            }

            Log("Logger initiated.");
        }

        public static void Log(string line)
        {
            WriteToTextFile.AddLineToFile(_fullLogPath, DateTime.Now + " " + line);
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