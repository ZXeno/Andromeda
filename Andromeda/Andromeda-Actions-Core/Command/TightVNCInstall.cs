using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Threading;
using Andromeda_Actions_Core.Infrastructure;
using Andromeda_Actions_Core.Model;

namespace Andromeda_Actions_Core.Command
{
    public class TightVNCInstall : Action
    {
        private ConnectionOptions _connOps;
        private CredToken _creds;
        private const string DestinationDirectory = "\\C$\\temp\\";
        private const string TightVncInstallerFileName = "tightvnc-setup-64bit.msi";

        public TightVNCInstall()
        {
            ActionName = "TightVNC Install";
            Description = "Installs the version of TightVNC from the components directory. [Requires Credentials]";
            Category = ActionGroup.Other;
            _connOps = new ConnectionOptions();
        }

        public override void RunCommand(string rawDeviceList)
        {
            List<string> devlist = ParseDeviceList(rawDeviceList);
            List<string> failedlist = new List<string>();

            _creds = CredentialManager.Instance.UserCredentials;

            if (!CredentialManager.Instance.CredentialsAreValid)
            {
                ResultConsole.AddConsoleLine("You must login for this command to work.");
                ResultConsole.AddConsoleLine(ActionName + "was canceled due to improper credentials.");
                Logger.Log("Invalid credentials. Action: " + ActionName + " canceled.");
                return;
            }

            string cmdToRun = "MsiExec.exe /i C:\\temp\\" + TightVncInstallerFileName +
                @" /quiet /norestart ADDLOCAL=Server SET_USEVNCAUTHENTICATION=1 VALUE_OF_USEVNCAUTHENTICATION=1 SET_PASSWORD=1 VALUE_OF_PASSWORD=PASS SET_REMOVEWALLPAPER=1 VALUE_OF_REMOVEWALLPAPER=0";
            Logger.Log("Running TightVNC Install with following command line parameters: " + cmdToRun);

            try
            {
                foreach (var device in devlist)
                {
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    if (!VerifyDeviceConnectivity(device))
                    {
                        failedlist.Add(device);
                        ResultConsole.Instance.AddConsoleLine("Device " + device + " failed connection verification. Added to failed list.");
                        Logger.Log("Device " + device + " failed connection verification. Added to failed list.");
                        continue;
                    }

                    if (!File.Exists("\\\\" + device + DestinationDirectory + TightVncInstallerFileName))
                    {
                        if (!Directory.Exists("\\\\" + device + DestinationDirectory))
                        {
                            Directory.CreateDirectory("\\\\" + device + DestinationDirectory);
                        }

                        File.Copy(Config.ComponentDirectory + "\\" + TightVncInstallerFileName, "\\\\" + device + DestinationDirectory + TightVncInstallerFileName);
                    }

                    RunPsExecCommand.RunOnDeviceWithAuthentication(device, cmdToRun, _creds);

                    Thread.Sleep(500);

                    File.Delete("\\\\" + device + DestinationDirectory + TightVncInstallerFileName);

                    Thread.Sleep(500);
                }
            }
            catch (OperationCanceledException e)
            {
                ResultConsole.AddConsoleLine("Operation " + ActionName + " canceled.");
                Logger.Log("Operation " + ActionName + " canceled by user request. " + e.Message);
                ResetCancelToken();
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}