using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Text;
using System.Windows;
using Andromeda.Model;

namespace Andromeda.Command
{
    public class RemoveHealthcast : Action
    {
        private ConnectionOptions _connOps;
        private CredToken _creds;
        private const string RemoveVNCFailedList = "RemoveHealthcast_Failed_W#_Log.txt";

        public RemoveHealthcast()
        {
            ActionName = "Remove Healthcast";
            Description = "Removes Healthcast from the specified computer(s).[Requires Credentials]";
            Category = ActionGroup.Other;
            _connOps = new ConnectionOptions();
        }

        public override void RunCommand(string a)
        {
            List<string> devlist = ParseDeviceList(a);
            List<string> successList = GetPingableDevices.GetDevices(devlist);
            List<string> failedlist = new List<string>();
            _creds = Program.CredentialManager.UserCredentials;

            if (!ValidateCredentials(_creds))
            {
                ResultConsole.AddConsoleLine("You must enter your username and password for this command to work.");
                ResultConsole.AddConsoleLine(ActionName + " was canceled due to improper credentials.");
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
                    var procquery1 = new SelectQuery("select * from Win32_process where name='XAUCM.exe'");
                    var procquery2 = new SelectQuery("select * from Win32_process where name='iexplore.exe'");
                    var productquery0 = new SelectQuery("select * from Win32_product where identifyingnumber='{14035D5A-FBF9-4EF8-8EA8-7B2793BFCC1C}'");
                    var productquery1 = new SelectQuery("select * from Win32_product where identifyingnumber='{B9059C25-4FF6-4E02-8309-B7AA51DA8197}'");
                    var productquery2 = new SelectQuery("select * from Win32_product where identifyingnumber='{FB9E8E8D-B862-4DE5-BA8C-9D158A406AE1}'");
                    ObjectQuery rebootQuery = new SelectQuery("Win32_OperatingSystem");

                    using (var searcher = new ManagementObjectSearcher(remote, procquery1))
                    {
                        foreach (ManagementObject process in searcher.Get()) // this is the fixed line
                        {
                            process.InvokeMethod("Terminate", null);
                            ResultConsole.AddConsoleLine("Called process terminate (" + process["Name"] + ") on device " + d + ".");
                            Logger.Log("Called process terminate (" + process["Name"] + ") on device " + d + ".");
                        }
                    }

                    using (var searcher = new ManagementObjectSearcher(remote, procquery2))
                    {
                        foreach (ManagementObject process in searcher.Get()) // this is the fixed line
                        {
                            process.InvokeMethod("Terminate", null);
                            ResultConsole.AddConsoleLine("Called process terminate (" + process["Name"] + ") on device " + d + ".");
                            Logger.Log("Called process terminate (" + process["Name"] + ") on device " + d + ".");
                        }
                    }

                    using (var searcher = new ManagementObjectSearcher(remote, productquery0))
                    {
                        foreach (ManagementObject product in searcher.Get()) // this is the fixed line
                        {
                            product.InvokeMethod("uninstall", null);
                            ResultConsole.AddConsoleLine("Called webplugin uninstall on device " + d + ".");
                            Logger.Log("Called uninstall on device " + d + ".");
                        }
                    }

                    using (var searcher = new ManagementObjectSearcher(remote, productquery1))
                    {
                        foreach (ManagementObject product in searcher.Get()) // this is the fixed line
                        {
                            product.InvokeMethod("uninstall", null);
                            ResultConsole.AddConsoleLine("Called proxcard uninstall on device " + d + ".");
                            Logger.Log("Called uninstall on device " + d + ".");
                        }
                    }

                    using (var searcher = new ManagementObjectSearcher(remote, productquery2))
                    {
                        foreach (ManagementObject product in searcher.Get()) // this is the fixed line
                        {
                            product.InvokeMethod("uninstall", null);
                            ResultConsole.AddConsoleLine("Called exactaccess uninstall on device " + d + ".");
                            Logger.Log("Called uninstall on device " + d + ".");
                        }
                    }

                    // Remove the remaining data regarding healthcast
                    var allProgramsFolder = "\\\\" + d + "\\C$\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\HealthCast";
                    var programDataHealthcast = "\\\\" + d + "\\C$\\ProgramData\\HealthCast";

                    if (Directory.Exists(allProgramsFolder))
                    {
                        try
                        {
                            Directory.Delete(allProgramsFolder, true);
                            Logger.Log("Remvoed directory " + allProgramsFolder);
                            ResultConsole.AddConsoleLine("Removed Healthcast \"All Programs\" directory on " + d);
                        }
                        catch (Exception e)
                        {
                            Logger.Log("There was an exception deleting the Healthcast \"All Programs\" directory on " + d + "\n Exception: " + e.Message + " Inner exception: " + e.InnerException);
                            ResultConsole.AddConsoleLine("There was an exception trying to delete the Healthcast \"All Programs\" directory on " + d +": " + e.Message);
                            ResultConsole.AddConsoleLine("See log for more details.");
                        }
                    }

                    if (Directory.Exists(programDataHealthcast))
                    {
                        try
                        {
                            Directory.Delete(programDataHealthcast, true);
                            Logger.Log("Remvoed directory " + programDataHealthcast);
                            ResultConsole.AddConsoleLine("Removed Healthcast \"ProgramData\" directory on " + d);
                        }
                        catch (Exception e)
                        {
                            Logger.Log("There was an exception deleting the Healthcast \"ProgramData\" directory on " + d + "\n Exception: " + e.Message + " Inner exception: " + e.InnerException);
                            ResultConsole.AddConsoleLine("There was an exception trying to delete the Healthcast \"ProgramData\" directory on " + d + ": " + e.Message);
                            ResultConsole.AddConsoleLine("See log for more details.");
                        }
                    }

                    // REBOOT!
                    using (var searcher = new ManagementObjectSearcher(remote, rebootQuery))
                    {
                        foreach (ManagementObject ro in searcher.Get()) // this is the fixed line
                        {
                            ManagementBaseObject inParams = ro.GetMethodParameters("Win32Shutdown");

                            // Add the input parameters.
                            inParams["Flags"] = 6;

                            // Execute the method and obtain the return values.
                            ManagementBaseObject outParams = ro.InvokeMethod("Win32Shutdown", inParams, null);

                            ResultConsole.AddConsoleLine("Reboot returned with value " + WMIFuncs.GetProcessReturnValueText(Convert.ToInt32(outParams["ReturnValue"])));
                        }
                    }
                }
                else
                {
                    ResultConsole.AddConsoleLine("Error connecting to WMI scope " + d + ". Process aborted for this device.");
                    Logger.Log("Error connecting to WMI scope " + d + ". Process aborted for this device.");
                    failedlist.Add(d);
                }

                ProgressData.OnUpdateProgressBar(1);

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