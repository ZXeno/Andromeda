﻿using System.Collections.Generic;
using System.Linq;
using System.Management;
using Andromeda.Infrastructure;
using Andromeda.Model;

namespace Andromeda.Logic.Command
{
    public class TightVNCRemove : Action
    {
        private ConnectionOptions _connOps;
        private readonly string _processName = "tvnserver.exe";

        public TightVNCRemove()
        {
            ActionName = "Remove TightVNC";
            Description = "Removes TightVNC from the specified computers.[Requires Credentials]";
            Category = ActionGroup.Other;
            _connOps = new ConnectionOptions();
        }

        public override void RunCommand(string a)
        {
            List<string> devlist = ParseDeviceList(a);
            List<string> confirmedConnectionList = GetPingableDevices.GetDevices(devlist);
            List<string> failedlist = new List<string>();

            if (!Program.CredentialManager.CredentialsAreValid)
            {
                ResultConsole.AddConsoleLine("You must enter your username and password for this command to work.");
                ResultConsole.AddConsoleLine(ActionName + "was canceled due to invalid credentials.");
                Logger.Log("Tried to run " + ActionName + " but there were no credentials added.");
                return;
            }

            _connOps.Username = Program.CredentialManager.UserCredentials.User;
            _connOps.SecurePassword = Program.CredentialManager.UserCredentials.SecurePassword;
            _connOps.Impersonation = ImpersonationLevel.Impersonate;

            foreach (var device in confirmedConnectionList)
            {
                var remote = WMIFuncs.ConnectToRemoteWMI(device, WMIFuncs.RootNamespace, _connOps);
                if (remote != null)
                {
                    var procquery = new SelectQuery("select * from Win32_process where name = '" + _processName + "'");
                    var productquery = new SelectQuery("select * from Win32_product where name='TightVNC'");

                    using (var searcher = new ManagementObjectSearcher(remote, procquery))
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
                else
                {
                    ResultConsole.AddConsoleLine("Error connecting to WMI scope " + device + ". Process aborted for this device.");
                    Logger.Log("Error connecting to WMI scope " + device + ". Process aborted for this device.");
                    failedlist.Add(device);
                }

            }

            if (failedlist.Count > 0)
            {
                Logger.WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}