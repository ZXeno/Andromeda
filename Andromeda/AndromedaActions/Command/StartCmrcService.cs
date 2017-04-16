using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading.Tasks;
using AndromedaCore.Infrastructure;
using AndromedaCore.Managers;
using Action = AndromedaCore.Action;

namespace AndromedaActions.Command
{
    public class StartCmrcService : Action
    {
        public StartCmrcService(ILoggerService logger, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices) : base(logger, networkServices, fileAndFolderServices)
        {
            ActionName = "Start CMRCService";
            Description = "Starts the CMRC Service on remote device(s).";
            Category = "SCCM";
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
                        ResultConsole.Instance.AddConsoleLine(
                            $"Device {device} failed connection verification. Added to failed list.");
                        return;
                    }

                    var sc = new ServiceController("CMRCService", device);

                    sc.Start();
                    ResultConsole.AddConsoleLine($"CMRCService started on {device}");
                });


            }
            catch (OperationCanceledException e)
            {
                ResetCancelToken(ActionName, e);
            }
            catch (Exception ex)
            {
                ResultConsole.AddConsoleLine($"Ended with error: {ex.Message}");
                Logger.LogError(ex.Message, ex);
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}