using System;
using System.Collections.Generic;
using System.Management;
using Andromeda.Model;

namespace Andromeda.Logic.Command
{
    public class UninstallXceleraMonitor : Action
    {
        private ConnectionOptions _connOps;
        private CredToken _creds;

        public UninstallXceleraMonitor()
        {
            ActionName = "Remove Xcelera Monitor";
            Description = "Removes Xcelera Monitor from the specified computers.[Requires Credentials]";
            Category = ActionGroup.Other;
            _connOps = new ConnectionOptions();
        }

        public override void RunCommand(string rawDeviceList)
        {
            List<string> devlist = ParseDeviceList(rawDeviceList);
            List<string> confirmedConnectionList = GetPingableDevices.GetDevices(devlist);
            List<string> failedlist = new List<string>();

            UpdateProgressBarForFailedConnections(devlist, confirmedConnectionList);

            _creds = Program.CredentialManager.UserCredentials;

            if (!ValidateCredentials(_creds))
            {
                ResultConsole.AddConsoleLine("You must enter your username and password for this command to work.");
                ResultConsole.AddConsoleLine("Remove Xcelera Monitor was canceled due to improper credentials.");
                Logger.Log("Invalid credentials entered. Action canceled.");
                return;
            }

            _connOps.Username = _creds.User;
            _connOps.SecurePassword = _creds.SecurePassword;
            _connOps.Impersonation = ImpersonationLevel.Impersonate;

            foreach (var device in confirmedConnectionList)
            {
                var remote = WMIFuncs.ConnectToRemoteWMI(device, WMIFuncs.RootNamespace, _connOps);
                if (remote != null)
                {
                    try
                    {
                        var procquery1 = new SelectQuery("select * from Win32_process where name='XceleraMonitorService.exe'");
                        var procquery2 = new SelectQuery("select * from Win32_process where name='XceleraMonitorUtility.exe'");
                        var productquery = new SelectQuery("select * from Win32_product where name='Xcelera Monitor'");

                        using (var searcher = new ManagementObjectSearcher(remote, procquery1))
                        {
                            foreach (ManagementObject process in searcher.Get()) // this is the fixed line
                            {
                                process.InvokeMethod("Terminate", null);
                                ResultConsole.AddConsoleLine("Called process terminate (" + process["Name"] + ") on device " + device + ".");
                                Logger.Log("Called process terminate (" + process["Name"] + ") on device " + device + ".");
                            }
                        }

                        using (var searcher = new ManagementObjectSearcher(remote, procquery2))
                        {
                            foreach (ManagementObject process in searcher.Get()) // this is the fixed line
                            {
                                process.InvokeMethod("Terminate", null);
                                ResultConsole.AddConsoleLine("Called process terminate (" + process["Name"] + ") on device " + device + ".");
                                Logger.Log("Called process terminate (" + process["Name"] + ") on device " + device + ".");
                            }
                        }

                        using (var searcher = new ManagementObjectSearcher(remote, productquery))
                        {
                            foreach (ManagementObject product in searcher.Get()) // this is the fixed line
                            {
                                product.InvokeMethod("uninstall", null);
                                ResultConsole.AddConsoleLine("Called uninstall on device " + device + ".");
                                Logger.Log("Called uninstall on device " + device + ".");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ActionName + " threw exception " + ex.Message + ". With inner exception message: " + ex.InnerException);
                        ResultConsole.AddConsoleLine(ActionName + " threw an exception: " + ex.Message);
                        failedlist.Add(device);
                    }
                    
                }
                else
                {
                    ResultConsole.AddConsoleLine("Error connecting to WMI scope " + device + ". Process aborted for this device.");
                    Logger.Log("Error connecting to WMI scope " + device + ". Process aborted for this device.");
                    failedlist.Add(device);
                }

                ProgressData.OnUpdateProgressBar(1);
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}