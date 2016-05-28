using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using Andromeda_Actions_Core.Infrastructure;
using Andromeda_Actions_Core.Model;

namespace Andromeda_Actions_Core
{
    public abstract class Action : IAction
    {
        protected ResultConsole ResultConsole => ResultConsole.Instance;
        protected Configuration Config => ConfigManager.CurrentConfig;

        public CancellationTokenSource CancellationToken { get; protected set; }
        public event EventHandler CancellationRequest;
        public void OnCancellationRequest(object sender, EventArgs e)
        {
            CancellationRequest?.Invoke(sender, e);
        }

        public string ActionName { get; protected set; }
        public string Description { get; protected set; }
        public ActionGroup Category { get; protected set; }

        protected Action()
        {
            CancellationToken = new CancellationTokenSource();
        }

        public abstract void RunCommand(string rawDeviceList);

        // Used for returning the name of the command to the GUI
        public override string ToString() { return ActionName; }

        // Return a list of devices from the string list of the GUI
        protected List<string> ParseDeviceList(string list)
        {
            var devList = new List<string>(list.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));

            var resultList = new List<string>();

            foreach (var d in devList)
            {
                var t = d;

                t = new string(t.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray());

                resultList.Add(t);
            }

            return resultList;
        }

        protected bool VerifyDeviceConnectivity(string device)
        {
            try
            {
                return NetworkConnections.Pingable(device) == IPStatus.Success;
            }
            catch (Exception)
            {
                return false;
            }
            
        }

        protected void ResetCancelToken()
        {
            CancellationToken.Dispose();
            CancellationToken = new CancellationTokenSource();
        }

        protected void CleanDirectory(string device, string path)
        {
            var fullPath = $"\\\\{device}\\C${path}";

            try
            {
                Directory.Delete(fullPath, true);
                Logger.Log($"Cleaned directory {fullPath}");
            }
            catch (Exception ex)
            {
                ResultConsole.AddConsoleLine($"Failed to clean directory {fullPath}. Due to exception {ex.Message}");
                Logger.Log($"Failed to clean directory {fullPath}. Due to exception {ex.Message} Inner exception: {ex.InnerException}");
            }
        }

        protected bool ValidateDirectoryExists(string device, string path)
        {
            try
            {
                return Directory.Exists($"\\\\{device}\\C$\\{path}");
            }
            catch (Exception ex)
            {
                ResultConsole.AddConsoleLine($"There was an exception when validating the directory {path} for machine: {device}");
                ResultConsole.AddConsoleLine(ex.Message);
                Logger.Log($"{ActionName} failed to validate directory: \\\\{device}\\C$\\{path}");
                return false;
            }
        }

        protected bool ValidateFileExists(string device, string path)
        {
            try
            {
                return File.Exists($"\\\\{device}\\C${path}");
            }
            catch (Exception ex)
            {
                ResultConsole.AddConsoleLine($"There was an exception when validating the file {path} for machine: {device}");
                ResultConsole.AddConsoleLine(ex.Message);
                Logger.Log($"{ActionName} failed to validate file: \\\\{device}\\C$\\{path}");
                return false;
            }
        }

        protected void WriteToFailedLog(string actionName, List<string> failedList)
        {
            var logFile = actionName.Replace(" ", "_") + "_failed_log.txt";
            var path = $"{Config.ResultsDirectory}\\{logFile}";
            var sb = new StringBuilder();

            if (File.Exists(path))
            {
                File.Delete(path);
                Logger.Log($"Deleted file {path}");
            }

            foreach (var failed in failedList)
            {
                sb.AppendLine(failed);
            }

            using (var outfile = new StreamWriter(Config.ResultsDirectory + "\\" + logFile, true))
            {
                try
                {
                    outfile.WriteAsync(sb.ToString());
                    Logger.Log($"Wrote \"{actionName}\" results to file {path}");
                    ResultConsole.AddConsoleLine($"There were {failedList.Count} computers that failed the process. They have been recorded in the log at {path}");
                }
                catch (Exception e)
                {
                    Logger.Log($"Unable to write to {path}. Error: {e.Message}");
                    ResultConsole.AddConsoleLine($"There were {failedList.Count} computers that failed the process. However, there was an exception attempting to write to the failed log file.");
                }
            }
        }
    }
}