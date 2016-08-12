using System;
using System.Collections.Generic;
using System.Management;
using AndromedaCore;
using AndromedaCore.Infrastructure;
using AndromedaCore.Managers;
using Action = AndromedaCore.Action;

namespace AndromedaActions.Command
{
    public class TightVNCRemove : Action
    {
        private readonly string _processName = "tvnserver.exe";
        private readonly string _productName = "TightVNC";

        private readonly IWmiServices _wmiServices;

        public TightVNCRemove(ILoggerService logger, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices, IWmiServices wmiServices) 
            : base(logger, networkServices, fileAndFolderServices)
        {
            ActionName = "TightVNC Remove";
            Description = "Removes TightVNC from the specified computers. [Requires Credentials]";
            Category = "Windows Management";

            _wmiServices = wmiServices;
        }

        public override void RunCommand(string a)
        {
            var devlist = ParseDeviceList(a);
            var failedlist = new List<string>();

            if (!CredentialManager.Instance.CredentialsAreValid)
            {
                ResultConsole.AddConsoleLine("You must enter your username and password for this command to work.");
                ResultConsole.AddConsoleLine($"{ActionName} was canceled due to invalid credentials.");
                Logger.LogMessage($"Tried to run {ActionName} but there were no credentials added.");
                return;
            }

            var connOps = new ConnectionOptions
            {
                Username = CredentialManager.Instance.UserCredentials.User,
                SecurePassword = CredentialManager.Instance.UserCredentials.SecurePassword,
                Impersonation = ImpersonationLevel.Impersonate
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
                        var procquery =
                            new SelectQuery($"select * from Win32_process where name = '{_processName}'");
                        var productquery = new SelectQuery($"select * from Win32_product where name='{_productName}'");

                        using (var searcher = new ManagementObjectSearcher(remote, procquery))
                        {
                            foreach (ManagementObject process in searcher.Get()) // this is the fixed line
                            {
                                process.InvokeMethod("Terminate", null);
                                ResultConsole.AddConsoleLine($"Called process terminate ({process["Name"]}) on device {device}.");
                                Logger.LogMessage($"Called process terminate ({process["Name"]}) on device {device}.");
                            }
                        }

                        using (var searcher = new ManagementObjectSearcher(remote, productquery))
                        {
                            foreach (ManagementObject product in searcher.Get()) // this is the fixed line
                            {
                                product.InvokeMethod("uninstall", null);
                                ResultConsole.AddConsoleLine($"Called uninstall on device {device}.");
                                Logger.LogMessage($"Called uninstall on device {device}.");
                            }
                        }
                    }
                    else
                    {
                        ResultConsole.AddConsoleLine($"Error connecting to WMI scope {device}. Process aborted for this device.");
                        Logger.LogWarning($"Error connecting to WMI scope {device}. Process aborted for this device.", null);
                        failedlist.Add(device);
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