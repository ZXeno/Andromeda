using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Text;
using System.Windows;
using Andromeda.Model;

namespace Andromeda.Command
{
    public class RemoveTightVNC : Action
    {
        private ConnectionOptions _connOps;
        private CredToken _creds;
        private readonly string _processName = "tvnserver.exe";
        private const string RemoveVNCFailedList = "RemoveVNC_Failed_W#_Log.txt";

        public RemoveTightVNC()
        {
            ActionName = "Remove TightVNC";
            Description = "Removes TightVNC from the specified computers.[Requires Credentials]";
            Category = ActionGroup.Other;
            _connOps = new ConnectionOptions();
        }

        public override void RunCommand(string deviceList)
        {
            List<string> devlist = ParseDeviceList(deviceList);
            List<string> successList = GetPingableDevices.GetDevices(devlist);
            List<string> failedlist = new List<string>();
            _creds = Program.CredentialManager.UserCredentials;

            if (!ValidateCredentials(_creds))
            {
                ResultConsole.AddConsoleLine("You must enter your username and password for this command to work.");
                ResultConsole.AddConsoleLine("Remove TightVNC was canceled due to improper credentials.");
                Logger.Log("Invalid credentials entered. Action canceled.");
                return;
            }

            _connOps.Username = _creds.User;
            _connOps.SecurePassword = _creds.SecurePassword;
            _connOps.Impersonation = ImpersonationLevel.Impersonate;

            foreach (var d in successList)
            {
                var remote = WMIFuncs.ConnectToRemoteWMI(d, WMIFuncs.RootNamespace, _connOps);
                if (remote != null)
                {
                    var procquery = new SelectQuery("select * from Win32_process where name = '" + _processName + "'");
                    var productquery = new SelectQuery("select * from Win32_product where name='TightVNC'");

                    using (var searcher = new ManagementObjectSearcher(remote, procquery))
                    {
                        foreach (ManagementObject process in searcher.Get()) // this is the fixed line
                        {
                            process.InvokeMethod("Terminate", null);
                            ResultConsole.AddConsoleLine("Called process terminate (" + process["Name"] + ") on device " + d + ".");
                            Logger.Log("Called process terminate (" + process["Name"] + ") on device " + d + ".");
                        }
                    }

                    using (var searcher = new ManagementObjectSearcher(remote, productquery))
                    {
                        foreach (ManagementObject product in searcher.Get()) // this is the fixed line
                        {
                            product.InvokeMethod("uninstall", null);
                            ResultConsole.AddConsoleLine("Called uninstall on device " + d + ".");
                            Logger.Log("Called uninstall on device " + d + ".");
                        }
                    }
                }
                else
                {
                    ResultConsole.AddConsoleLine("Error connecting to WMI scope " + d + ". Process aborted for this device.");
                    Logger.Log("Error connecting to WMI scope " + d + ". Process aborted for this device.");
                    failedlist.Add(d);
                }
            }

            if (failedlist.Count > 0)
            {
                ResultConsole.AddConsoleLine("There were " + failedlist.Count + "computers that failed the process. They have been recorded in the log.");
                StringBuilder sb = new StringBuilder();

                if (File.Exists(Config.ResultsDirectory + "\\" + RemoveVNCFailedList))
                {
                    File.Delete(Config.ResultsDirectory + "\\" + RemoveVNCFailedList);
                    Logger.Log("Deleted file " + Config.ResultsDirectory + "\\" + RemoveVNCFailedList);
                }

                foreach (var failed in failedlist)
                {
                    sb.AppendLine(failed);
                }

                using (StreamWriter outfile = new StreamWriter(Config.ResultsDirectory + "\\" + RemoveVNCFailedList, true))
                {
                    try
                    {
                        outfile.WriteAsync(sb.ToString());
                        Logger.Log("Wrote \"Remove TightVNC Failed\" results to file " + Config.ResultsDirectory + "\\" + RemoveVNCFailedList);
                    }
                    catch (Exception e)
                    {
                        Logger.Log("Unable to write to " + RemoveVNCFailedList + ". \n" + e.InnerException);
                        MessageBox.Show("Unable to write to " + RemoveVNCFailedList + ". \n" + e.InnerException);
                    }
                }
            }
        }
    }
}