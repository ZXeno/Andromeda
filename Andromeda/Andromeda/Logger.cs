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
            _logFileName = DateTime.Today.Month + "_" + DateTime.Today.Day + "_" + DateTime.Today.Year + "__" +
                           DateTime.Now.Hour + "-" + DateTime.Now.Minute + "_log.txt";

            _fullLogPath = _logFilePath + "\\" + _logFileName;

            ValidateDestinationExists();

            WriteToTextFile.CreateNewLogFile(_fullLogPath);
            Log("Logger initiated");
        }

        public static void Log(string line)
        {
            WriteToTextFile.AddLineToFile(_logFilePath + "\\" + _logFileName, DateTime.Now + " " + line);
        }

        private static void ValidateDestinationExists()
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