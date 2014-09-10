using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using System.Windows;

namespace Andromeda.Command
{
    public class RunRemoteCommand : Action
    {

        NetworkConnections netconn;

        public RunRemoteCommand()
        {
            ActionName = "Run Command Remotely";
            Desctiption = "Run any console command remotely, as specified credentials.";
            Category = ActionGroup.Other;
            netconn = new NetworkConnections();
        }


        public override string RunCommand(string deviceList) 
        {
            string wmiscope = "\\root\\cimv2";
            string processToRun = "";
            List<string> devices = ParseDeviceList(deviceList);
            ConnectionOptions connOps = new ConnectionOptions();
            connOps.Username = CredentialManager.GetUser();
            connOps.Password = CredentialManager.GetPass();

            CLI_Prompt newPrompt = new CLI_Prompt();
            newPrompt.ShowDialog();

            try
            {
                if (!newPrompt.WasCanceled)
                {
                    processToRun = newPrompt.TextBoxContents;
                    newPrompt = null;
                }
                else if (newPrompt.WasCanceled)
                {
                    newPrompt = null;
                    return "Run Command Remotely action was canceled.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error running this command. \n " + ex.Message);
                return "Command failed with exception error caught: \n" + ex.Message;
            }

            foreach (string d in devices)
            {
                ResultConsole.AddConsoleLine(RunOnDevice(d, wmiscope, processToRun, connOps));
            }

            return "Completed"; 
        }

        private string RunOnDevice(string d, string scope, string process, ConnectionOptions options)
        {
            ManagementScope deviceWMI;
            string result = "";
            var processToRun = new[] { process };

            if (netconn.CheckWMIAccessible(d, scope))
            {
                deviceWMI = netconn.ConnectToRemoteWMI(d, scope);
                ManagementClass wmiProcess = new ManagementClass(deviceWMI, new ManagementPath("Win32_Process"), new ObjectGetOptions());
                wmiProcess.InvokeMethod("Create", processToRun);
            }
            else
            {
                result = "Unable to connect to device " + d + ".";
            }

            return result;
        }
    }
}
