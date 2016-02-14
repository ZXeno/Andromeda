﻿using System;
using System.Collections.Generic;
using System.Windows;
using Andromeda.Infrastructure;
using Andromeda.Model;
using Andromeda.ViewModel;

namespace Andromeda.Logic.Command
{
    public class RunRemoteCommand : Action
    {
        public RunRemoteCommand()
        {
            ActionName = "Run Command Remotely";
            Description = "Run any console command remotely. (use /c with any CMD.exe commands) [Requires Credentials]";
            Category = ActionGroup.Other;
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
            
            string cmdToRun = "";
            var newPrompt = new CliViewModel();
            newPrompt.OpenNewPrompt();

            
            Logger.Log("Opening CLI prompt.");

            try
            {
                if (newPrompt.Result)
                {
                    cmdToRun = newPrompt.TextBoxContents;
                    newPrompt = null;

                    foreach (var device in confirmedConnectionList)
                    {
                        RunOnDevice(device, cmdToRun);
                    }

                    if (failedlist.Count > 0)
                    {
                        Logger.WriteToFailedLog(ActionName, failedlist);
                    }
                }
                else //if (newPrompt.WasCanceled)
                {
                    newPrompt = null;
                    Logger.Log("Run Command Remotely action was canceled.");
                    ResultConsole.AddConsoleLine("Run Command Remotely action was canceled.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error running this command. \n " + ex.Message);
                ResultConsole.AddConsoleLine("Command failed with exception error caught: \n" + ex.Message);
            }
        }

        private void RunOnDevice(string device, string commandline)
        {
            RunPSExecCommand.RunOnDeviceWithAuthentication(device, commandline, Program.CredentialManager.UserCredentials);
        }

    }
}

