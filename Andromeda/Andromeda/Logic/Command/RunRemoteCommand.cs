using System;
using System.Collections.Generic;
using System.Management;
using System.Windows;
using Andromeda.Model;
using Andromeda.ViewModel;

namespace Andromeda.Command
{
    public class RunRemoteCommand : Action
    {
        private CredToken _creds;
        ConnectionOptions connOps;

        public RunRemoteCommand()
        {
            ActionName = "Run Command Remotely";
            Description = "Run any console command remotely, as specified credentials. (use /c with any CMD.exe commands)";
            Category = ActionGroup.Other;
            connOps = new ConnectionOptions();
        }

        public override void RunCommand(string deviceList)
        {
            List<string> devlist = ParseDeviceList(deviceList);
            List<string> successList = GetPingableDevices.GetDevices(devlist);
            _creds = Program.CredentialManager.UserCredentials;

            if (_creds.User == "" || _creds.User == "USERNAME" || _creds.User == "username")
            {
                ResultConsole.AddConsoleLine("You must enter your username and password for this command to work.");
                ResultConsole.AddConsoleLine("Run Remote Command was canceled due to improper credentials.");
                Logger.Log("Invalid credentials entered.");
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

                    foreach (var d in successList)
                    {
                        RunOnDevice(d, cmdToRun);
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

            _creds = null;
            newPrompt.Dispose();
            newPrompt = null;
        }

        private void RunOnDevice(string device, string commandline)
        {
            RunPSExecCommand.RunOnDeviceWithAuthentication(device, commandline, _creds);
        }

    }
}

