using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AndromedaActions.View;
using AndromedaActions.ViewModel;
using AndromedaCore;
using AndromedaCore.Infrastructure;
using AndromedaCore.Managers;
using Action = AndromedaCore.Action;

namespace AndromedaActions.Command
{
    public class FileCopy : Action
    {
        public FileCopy(ILoggerService logger, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices)
            : base(logger, networkServices, fileAndFolderServices)
        {
            ActionName = "File Copy";
            Description = "Copy file to all devices in the list.";
            Category = "Other";
        }

        public override void RunCommand(string rawDeviceList)
        {
            var devlist = ParseDeviceList(rawDeviceList);
            var failedlist = new List<string>();

            var fileCopyContext = new FileCopyPromptViewModel();
            var fileCopyPrompt = new FileCopyPrompt
            {
                DataContext = fileCopyContext
            };
            fileCopyPrompt.ShowDialog();

            if (!fileCopyContext.Result)
            {
                var msg = $"Action {ActionName} canceled by user.";
                Logger.LogMessage(msg);
                ResultConsole.AddConsoleLine(msg);
                return;
            }

            if (string.IsNullOrWhiteSpace(fileCopyContext.FilePath))
            {
                var msg = $"Action {ActionName} aborted: source path was empty.";
                Logger.LogMessage(msg);
                ResultConsole.AddConsoleLine(msg);
                return;
            }

            try
            {
                Parallel.ForEach(devlist, (device) =>
                {
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    if (!NetworkServices.VerifyDeviceConnectivity(device))
                    {
                        failedlist.Add(device);
                        ResultConsole.Instance.AddConsoleLine($"Device {device} failed connection verification. Added to failed list.");
                        return;
                    }

                    var fileName = fileCopyContext.FilePath.Split(new char[] {'\\'}).Last();

                    string destPath;
                    if (string.IsNullOrWhiteSpace(fileCopyContext.DestinationPath))
                    {
                        destPath = $"\\\\{device}\\C$\\";
                    }
                    else
                    {
                        if (fileCopyContext.DestinationPath.StartsWith("\\"))
                        {
                            fileCopyContext.DestinationPath = fileCopyContext.DestinationPath.Remove(0, 1);
                        }

                        if (fileCopyContext.DestinationPath.EndsWith("\\"))
                        {
                            fileCopyContext.DestinationPath = fileCopyContext.DestinationPath.Remove(fileCopyContext.DestinationPath.Length - 1, 1);
                        }

                        destPath = $"\\\\{device}\\C$\\{fileCopyContext.DestinationPath}\\";
                    }
                    
                    ResultConsole.AddConsoleLine($"Copying file {fileName} to device {device}");

                    try
                    {
                        if (FileAndFolderServices.ValidateDirectoryExists(device, fileCopyContext.DestinationPath, ActionName, Logger))
                        {
                            File.Copy(fileCopyContext.FilePath, destPath + fileName, fileCopyContext.Overwrite);
                            Logger.LogMessage($"Copied file {fileName} to {destPath}");
                        }
                        else if (fileCopyContext.CreateDestination)
                        {
                            Logger.LogMessage($"Creating directory {fileCopyContext.DestinationPath}");
                            Directory.CreateDirectory(destPath);

                            Thread.Sleep(100);

                            File.Copy(fileCopyContext.FilePath, destPath + fileName, fileCopyContext.Overwrite);
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
    }
}