using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using AndromedaCore.Infrastructure;
using AndromedaCore.Managers;
using Action = AndromedaCore.Action;

namespace AndromedaActions.Command
{
    public class ResetWindowsUpdateService : Action
    {
        private const string UpdateService = "wuauserv";
        private const string SoftwareDistributionPath = "Windows\\SoftwareDistribution";
        private readonly TimeSpan _timeouTimeSpan = new TimeSpan(0, 0, 0, 20);

        public ResetWindowsUpdateService(ILoggerService logger, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices) : base(logger, networkServices, fileAndFolderServices)
        {
            ActionName = "Reset Windows Update Service";
            Description = "Resets the Windows Update Service on a device and clears update cache.";
            Category = "Windows Management";
        }

        public override void RunCommand(string rawDeviceList)
        {
            var devlist = ParseDeviceList(rawDeviceList);
            var failedlist = new List<string>();

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

                    var sc = new ServiceController(UpdateService, device);

                    ResultConsole.Instance.AddConsoleLine($"Stopping update service on device {device}");
                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, _timeouTimeSpan);
                        ResultConsole.Instance.AddConsoleLine($"Update service stopped on device {device}");
                    }
                    else
                    {
                        ResultConsole.Instance.AddConsoleLine($"Update service not running on device {device}. Continuing...");
                    }

                    var remotepath = $"\\\\{device}\\C$\\{SoftwareDistributionPath}";

                    if (FileAndFolderServices.ValidateDirectoryExists(device, SoftwareDistributionPath, ActionName, Logger))
                    {
                        var contentsList = Directory.GetFileSystemEntries(remotepath).ToList();
                        if (contentsList?.Count > 0)
                        {
                            foreach (var str in contentsList)
                            {
                                var isdir = File.Exists(str);
                                if (isdir)
                                {
                                    File.Delete(str);
                                    continue;
                                }

                                Directory.Delete(str, true);
                            }
                        }
                    }

                    ResultConsole.Instance.AddConsoleLine($"Starting update service on device {device}");
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, _timeouTimeSpan);
                    ResultConsole.Instance.AddConsoleLine($"Update service started on device {device}");
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