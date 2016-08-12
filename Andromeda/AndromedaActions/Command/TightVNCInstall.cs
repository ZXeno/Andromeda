﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using AndromedaCore;
using AndromedaCore.Infrastructure;
using AndromedaCore.Managers;
using AndromedaCore.Model;
using Action = AndromedaCore.Action;

namespace AndromedaActions.Command
{
    public class TightVNCInstall : Action
    {
        private CredToken _creds => CredentialManager.Instance.UserCredentials;
        private const string DestinationDirectory = "\\C$\\temp\\";
        private const string TightVncInstallerFileName = "tightvnc-setup-64bit.msi";

        private readonly IPsExecServices _psExecServices;

        public TightVNCInstall(ILoggerService logger, INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices, IPsExecServices psExecServices)
            : base(logger, networkServices, fileAndFolderServices)
        {
            ActionName = "TightVNC Install";
            Description = "Installs the version of TightVNC from the components directory. [Requires Credentials]";
            Category = "Windows Management";

            _psExecServices = psExecServices;
        }

        public override void RunCommand(string rawDeviceList)
        {
            var devlist = ParseDeviceList(rawDeviceList);
            var failedlist = new List<string>();

            if (!CredentialManager.Instance.CredentialsAreValid)
            {
                ResultConsole.AddConsoleLine("You must login for this command to work.");
                ResultConsole.AddConsoleLine(ActionName + "was canceled due to improper credentials.");
                Logger.LogMessage("Invalid credentials. Action: " + ActionName + " canceled.");
                return;
            }

            string cmdToRun = $"MsiExec.exe /i C:\\temp\\{TightVncInstallerFileName} /quiet /norestart ADDLOCAL=Server SET_USEVNCAUTHENTICATION=1 VALUE_OF_USEVNCAUTHENTICATION=1 SET_PASSWORD=1 VALUE_OF_PASSWORD=PASS SET_REMOVEWALLPAPER=1 VALUE_OF_REMOVEWALLPAPER=0";
            Logger.LogMessage($"Running TightVNC Install with following command line parameters: {cmdToRun}");

            try
            {
                foreach (var device in devlist)
                {
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    if (!NetworkServices.VerifyDeviceConnectivity(device))
                    {
                        failedlist.Add(device);
                        ResultConsole.Instance.AddConsoleLine($"Device {device} failed connection verification. Added to failed list.");
                        Logger.LogWarning($"Device {device} failed connection verification. Added to failed list.", null);
                        continue;
                    }

                    try
                    {
                        if (!File.Exists($"\\\\{device}{DestinationDirectory}{TightVncInstallerFileName}"))
                        {
                            if (!Directory.Exists($"\\\\{device}{DestinationDirectory}"))
                            {
                                Directory.CreateDirectory($"\\\\{device}{DestinationDirectory}");
                            }

                            File.Copy($"{Config.ComponentDirectory}\\{TightVncInstallerFileName}", $"\\\\{device}{DestinationDirectory}{TightVncInstallerFileName}");
                        }
                    }
                    catch (Exception e)
                    {
                        var msg = $"There was an error while trying to copy the TightVNC installer to the remote device. Error: {e.Message}";
                        Logger.LogMessage(msg);
                        ResultConsole.AddConsoleLine(msg);
                        Thread.Sleep(500);
                        continue;
                    }
                    

                    _psExecServices.RunOnDeviceWithAuthentication(device, cmdToRun, _creds);

                    Thread.Sleep(500);

                    File.Delete($"\\\\{device}{DestinationDirectory}{TightVncInstallerFileName}");

                    Thread.Sleep(500);
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