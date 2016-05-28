using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Andromeda.View;
using Andromeda_Actions_Core.Infrastructure;
using Andromeda_Actions_Core.ViewModel;

namespace Andromeda_Actions_Core.Command
{
    public class FileCopy : Action
    {
        public FileCopy()
        {
            ActionName = "File Copy";
            Description = "Copy file to all devices in the list.";
            Category = ActionGroup.Other;
        }

        public override void RunCommand(string rawDeviceList)
        {
            List<string> devlist = ParseDeviceList(rawDeviceList);
            List<string> failedlist = new List<string>();

            var fileCopyContext = new FileCopyPromptViewModel();
            var fileCopyPrompt = new FileCopyPrompt
            {
                DataContext = fileCopyContext
            };
            fileCopyPrompt.ShowDialog();

            if (!fileCopyContext.Result)
            {
                Logger.Log($"Action {ActionName} canceled by user.");
                ResultConsole.AddConsoleLine($"Action {ActionName} canceled by user.");
                return;
            }

            if (string.IsNullOrWhiteSpace(fileCopyContext.FilePath))
            {
                Logger.Log($"Action {ActionName} aborted: source path was empty.");
                ResultConsole.AddConsoleLine("Aborting: Source path is empty.");
                return;
            }

            

            try
            {
                Parallel.ForEach(devlist, (device) =>
                {
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    if (!VerifyDeviceConnectivity(device))
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
                        if (ValidateDirectoryExists(device, fileCopyContext.DestinationPath))
                        {
                            File.Copy(fileCopyContext.FilePath, destPath + fileName, fileCopyContext.Overwrite);
                            Logger.Log($"Copied file {fileName} to {destPath}");
                        }
                        else if (fileCopyContext.CreateDestination)
                        {
                            Logger.Log($"Creating directory {fileCopyContext.DestinationPath}");
                            Directory.CreateDirectory(destPath);

                            Thread.Sleep(100);

                            File.Copy(fileCopyContext.FilePath, destPath + fileName, fileCopyContext.Overwrite);
                        }
                        else
                        {
                            ResultConsole.AddConsoleLine("Unable to copy, destination doesn't exist.");
                            Logger.Log("Unable to copy, destination doesn't exist.");
                        }
                    }
                    catch (Exception e)
                    {
                        ResultConsole.AddConsoleLine($"Unable to copy file {fileName}. Error: {e.Message}");
                        Logger.Log($"Unable to copy file {fileName}. Error: {e.Message}");
                    }
                });
            }
            catch (OperationCanceledException e)
            {
                ResultConsole.AddConsoleLine($"Operation {ActionName} canceled.");
                Logger.Log($"Operation {ActionName} canceled by user request. {e.Message}");
                ResetCancelToken();
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}