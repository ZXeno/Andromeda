using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using AndromedaCore.Infrastructure;
using AndromedaCore.Managers;
using AndromedaCore.Model;

namespace AndromedaCore
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

        protected INetworkServices NetworkServices;
        protected IFileAndFolderServices FileAndFolderServices;
        protected ILoggerService Logger;

        protected Action(ILoggerService logger, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices)
        {
            Logger = logger;
            NetworkServices = networkServices;
            FileAndFolderServices = fileAndFolderServices;

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

        protected void ResetCancelToken(string actionName, OperationCanceledException e)
        {
            ResultConsole.AddConsoleLine($"Operation {actionName} canceled.");
            Logger.LogMessage($"Operation {actionName} canceled by user request. {e.Message}");

            ResetCancelToken();
        }

        protected void ResetCancelToken()
        {
            CancellationToken.Dispose();
            CancellationToken = new CancellationTokenSource();
        }

        protected void WriteToFailedLog(string actionName, List<string> failedList)
        {
            var logFile = $"{actionName.Replace(" ", "_")}_failed_log.txt";
            var path = $"{Config.ResultsDirectory}\\{logFile}";
            var sb = new StringBuilder();

            if (File.Exists(path))
            {
                File.Delete(path);
                Logger.LogMessage($"Deleted file {path}");
            }

            foreach (var device in failedList)
            {
                sb.AppendLine(device);
            }

            try
            {
                FileAndFolderServices.WriteToTextFile(path, sb.ToString(), Logger);

                Logger.LogMessage($"Wrote \"{actionName}\" results to file {path}");
                ResultConsole.AddConsoleLine($"There were {failedList.Count} computers that failed the process. They have been recorded in the log at {path}");
            }
            catch (Exception e)
            {
                Logger.LogError($"Unable to write to {path}.", e);
                ResultConsole.AddConsoleLine($"There were {failedList.Count} computers that failed the process. However, there was an exception attempting to write to the failed log file.");
            }
        }
    }
}