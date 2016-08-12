using System;
using System.Collections.Generic;
using System.Management;
using AndromedaCore;
using AndromedaCore.Infrastructure;
using AndromedaCore.Managers;
using Action = AndromedaCore.Action;

namespace AndromedaActions.Command
{
    public class ForceLogOff : Action
    {
        //' Flag values:
        //' 0 - Log off
        //' 4 - Forced log off
        //' 1 - Shut down
        //' 5 - Forced shut down
        //' 2 - Reboot
        //' 6 - Forced reboot
        //' 8 - Power off
        //' 12 - Forced power off 

        private readonly IWmiServices _wmiServices;

        public ForceLogOff(ILoggerService logger, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices, IWmiServices wmiServices) 
            : base(logger, networkServices, fileAndFolderServices)
        {
            ActionName = "Force Log Off";
            Description = "Forces the remote user to log off.";
            Category = ActionGroup.WindowsManagement;

            _wmiServices = wmiServices;
        }

        public override void RunCommand(string rawDeviceList)
        {
            var devlist = ParseDeviceList(rawDeviceList);
            var failedlist = new List<string>();

            var connOps = new ConnectionOptions
            {
                EnablePrivileges = true
            };

            try
            {
                foreach (var device in devlist)
                {
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    if (!NetworkServices.VerifyDeviceConnectivity(device))
                    {
                        failedlist.Add(device);
                        ResultConsole.Instance.AddConsoleLine($"Device {device} failed connection verification. Added to failed list.");
                        continue;
                    }

                    var remote = _wmiServices.ConnectToRemoteWmi(device, _wmiServices.RootNamespace, connOps);
                    if (remote != null)
                    {
                        ObjectQuery query = new SelectQuery("Win32_OperatingSystem");

                        var searcher = new ManagementObjectSearcher(remote, query);
                        var queryCollection = searcher.Get();

                        foreach (var resultobject in queryCollection)
                        {
                            var ro = resultobject as ManagementObject;
                            // Obtain in-parameters for the method
                            var inParams = ro.GetMethodParameters("Win32Shutdown");

                            // Add the input parameters.
                            inParams["Flags"] = 4;

                            try
                            {
                                // Execute the method and obtain the return values.
                                var outParams = ro.InvokeMethod("Win32Shutdown", inParams, null);

                                ResultConsole.AddConsoleLine($"Returned with value {_wmiServices.GetProcessReturnValueText(Convert.ToInt32(outParams["ReturnValue"]))}");
                            }
                            catch (Exception e)
                            {
                                ResultConsole.AddConsoleLine($"Error running {ActionName} due to a .Net ManagementExcept error. There are likely no users logged on!");
                                Logger.LogWarning($"Error running {ActionName} due to a .Net ManagementExcept error.", e);
                            }
                        }
                    }
                    else
                    {
                        Logger.LogWarning($"There was an error connecting to WMI namespace on {device}", null);
                        ResultConsole.AddConsoleLine($"There was an error connecting to WMI namespace on {device}");
                    }
                }
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