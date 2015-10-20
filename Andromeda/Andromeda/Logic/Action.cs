using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Andromeda.Model;

namespace Andromeda.Logic
{
    public class Action
    {
        public ResultConsole ResultConsole { get { return ResultConsole.Instance; } }
        protected Configuration Config { get { return ConfigManager.CurrentConfig; } }
        protected NetworkConnections netConn;

        public string ActionName { get; protected set; }
        public string Description { get; protected set; }
        public ActionGroup Category { get; protected set; }

        // Single entry
        public virtual void RunCommand(string rawDeviceList) {  }

        // Used for returning the name of the command to the GUI
        public override string ToString() { return ActionName; }

        // Return a list of devices from the string list of the GUI
        public List<string> ParseDeviceList(string list)
        {
            List<string> devList = new List<string>(list.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));

            List<string> resultList = new List<string>();

            foreach (var d in devList)
            {
                var t = d;

                t = new string(t.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray());

                resultList.Add(t);
            }

            ProgressData.OnStartProgressBar(resultList.Count);

            return resultList;
        }

        protected void UpdateProgressBarForFailedConnections(List<string> deviceList, List<string> confirmedConnectionList)
        {
            var difflist = deviceList.Where(x => !confirmedConnectionList.Contains(x));

            foreach (var diff in difflist)
            {
                ProgressData.OnUpdateProgressBar(1);
            }
        }

        protected bool ValidateCredentials(CredToken credentialToken)
        {
            if (credentialToken == null||
                credentialToken.User == "" || 
                credentialToken.User == "USERNAME" || 
                credentialToken.User == "username")
            {
                return false;
            }

            return true;
        }

        protected void CleanDirectory(string device, string path)
        {
            var fullPath = "\\\\" + device + "\\C$" + path;

            try
            {
                Directory.Delete(fullPath, true);
                Logger.Log("Cleaned directory " + fullPath);
            }
            catch (Exception ex)
            {
                ResultConsole.AddConsoleLine("Failed to clean directory " + fullPath + ". Due to exception " + ex.Message);
                Logger.Log("Failed to clean directory " + fullPath + ". Due to exception " + ex.Message + " Inner exception: " + ex.InnerException);
            }
        }

        protected bool ValidateDirectoryExists(string device, string path)
        {
            try
            {
                return Directory.Exists("\\\\" + device + "\\C$\\" + path);
            }
            catch (Exception ex)
            {
                ResultConsole.AddConsoleLine("There was an exception when validating the directory" + path + " for machine: " + device);
                ResultConsole.AddConsoleLine(ex.Message);
                Logger.Log(ActionName + " failed to validate directory: \\\\" + device + "\\C$\\" + path);
                return false;
            }
        }

        protected bool ValidateFileExists(string device, string path)
        {
            try
            {
                return File.Exists("\\\\" + device + "\\C$" + path);
            }
            catch (Exception ex)
            {
                ResultConsole.AddConsoleLine("There was an exception when validating the file" + path + " for machine: " + device);
                ResultConsole.AddConsoleLine(ex.Message);
                Logger.Log(ActionName + " failed to validate file: \\\\" + device + "\\C$\\" + path);
                return false;
            }
        }

        protected void WriteToFailedLog(string actionName, List<string> failedList)
        {
            string logFile = actionName.Replace(" ", "_") + "_failed_log.txt";
            StringBuilder sb = new StringBuilder();

            if (File.Exists(Config.ResultsDirectory + "\\" + logFile))
            {
                File.Delete(Config.ResultsDirectory + "\\" + logFile);
                Logger.Log("Deleted file " + Config.ResultsDirectory + "\\" + logFile);
            }

            foreach (var failed in failedList)
            {
                sb.AppendLine(failed);
            }

            using (StreamWriter outfile = new StreamWriter(Config.ResultsDirectory + "\\" + logFile, true))
            {
                try
                {
                    outfile.WriteAsync(sb.ToString());
                    Logger.Log("Wrote \"" + actionName + "\" results to file " + Config.ResultsDirectory + "\\" + logFile);
                    ResultConsole.AddConsoleLine("There were " + failedList.Count + "computers that failed the process. They have been recorded in the log.");
                }
                catch (Exception e)
                {
                    Logger.Log("Unable to write to " + logFile + ". \n" + e.InnerException);
                    ResultConsole.AddConsoleLine("There were " + failedList.Count + "computers that failed the process. However, there was an exception attempting to write to the failed log file.");
                }
            }
        }
    }
}
