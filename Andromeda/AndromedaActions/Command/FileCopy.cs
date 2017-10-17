using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AndromedaActions.View;
using AndromedaActions.ViewModel;
using AndromedaCore.Infrastructure;
using AndromedaCore.Managers;
using AndromedaCore.ViewModel;
using Action = AndromedaCore.Action;

namespace AndromedaActions.Command
{
    public class FileCopy : Action
    {
        private readonly IWindowService _windowService;

        private List<string> _parsedListCache;
        private FileCopyPromptViewModel fileCopyContextCache;

        public FileCopy(ILoggerService logger, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices, IWindowService windowService)
            : base(logger, networkServices, fileAndFolderServices)
        {
            ActionName = "File Copy";
            Description = "Copy file to all devices in the list.";
            Category = "Other";

            _windowService = windowService;
            HasUserInterfaceElement = true;
            UiCallback = CallbackMethod;
        }

        public override void OpenUserInterfaceElement(string rawDeviceList)
        {
            _parsedListCache = ParseDeviceList(rawDeviceList);

            fileCopyContextCache = new FileCopyPromptViewModel();
            _windowService.ShowDialog<FileCopyPrompt>(fileCopyContextCache);

            if (!fileCopyContextCache.Result)
            {
                var msg = $"Action {ActionName} canceled by user.";
                Logger.LogMessage(msg);
                ResultConsole.AddConsoleLine(msg);
                CancellationToken.Cancel();
            }

            if (string.IsNullOrWhiteSpace(fileCopyContextCache.FilePath))
            {
                var msg = $"Action {ActionName} aborted: source path was empty.";
                Logger.LogMessage(msg);
                ResultConsole.AddConsoleLine(msg);
                CancellationToken.Cancel();
            }
        }

        private void CallbackMethod()
        {
            var failedlist = new List<string>();

            try
            {
                Parallel.ForEach(_parsedListCache, (device) =>
                {
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    if (!NetworkServices.VerifyDeviceConnectivity(device))
                    {
                        failedlist.Add(device);
                        ResultConsole.Instance.AddConsoleLine($"Device {device} failed connection verification. Added to failed list.");
                        return;
                    }

                    var fileName = fileCopyContextCache.FilePath.Split(new char[] { '\\' }).Last();

                    string destPath;
                    if (string.IsNullOrWhiteSpace(fileCopyContextCache.DestinationPath))
                    {
                        destPath = $"\\\\{device}\\C$\\";
                    }
                    else
                    {
                        if (fileCopyContextCache.DestinationPath.StartsWith("\\"))
                        {
                            fileCopyContextCache.DestinationPath = fileCopyContextCache.DestinationPath.Remove(0, 1);
                        }

                        if (fileCopyContextCache.DestinationPath.EndsWith("\\"))
                        {
                            fileCopyContextCache.DestinationPath = fileCopyContextCache.DestinationPath.Remove(fileCopyContextCache.DestinationPath.Length - 1, 1);
                        }

                        destPath = $"\\\\{device}\\C$\\{fileCopyContextCache.DestinationPath}\\";
                    }

                    ResultConsole.AddConsoleLine($"Copying file {fileName} to device {device}");

                    try
                    {
                        if (FileAndFolderServices.ValidateDirectoryExists(device, fileCopyContextCache.DestinationPath, ActionName, Logger))
                        {
                            File.Copy(fileCopyContextCache.FilePath, destPath + fileName, fileCopyContextCache.Overwrite);
                            Logger.LogMessage($"Copied file {fileName} to {destPath}");
                        }
                        else if (fileCopyContextCache.CreateDestination)
                        {
                            Logger.LogMessage($"Creating directory {fileCopyContextCache.DestinationPath}");
                            Directory.CreateDirectory(destPath);

                            Thread.Sleep(100);

                            File.Copy(fileCopyContextCache.FilePath, destPath + fileName, fileCopyContextCache.Overwrite);
                        }
                        else
                        {
                            ResultConsole.AddConsoleLine("Unable to copy, destination doesn't exist.");
                            Logger.LogMessage("Unable to copy, destination doesn't exist.");
                        }
                    }
                    catch (Exception e)
                    {
                        ResultConsole.AddConsoleLine($"Unable to copy file {fileName}. Error: {e.Message}");
                        Logger.LogWarning($"Unable to copy file {fileName}.", e);
                    }
                });
            }
            catch (OperationCanceledException e)
            {
                ResetCancelToken(ActionName, e);
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }

        public override void RunCommand(string rawDeviceList)
        {
            throw new NotImplementedException($"{ActionName} has a user interface element and does not utilize the RunCommand method interface.");
        }
    }
}