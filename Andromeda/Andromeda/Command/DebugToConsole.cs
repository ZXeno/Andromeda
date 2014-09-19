using System;
using System.Management;
using System.Collections.Generic;
using System.Windows;
using System.Text;

namespace Andromeda.Command
{
    class DebugToConsole : Action
    {
        NetworkConnections netconn;
        WMIFuncs wmi;
        ConnectionOptions connOps;

        public DebugToConsole()
        {
            ActionName = "Debug to Console";
            Desctiption = "For testing new commands, actions, and abilities.";
            Category = ActionGroup.Debug;
            netconn = new NetworkConnections();
            connOps = new ConnectionOptions();
            connOps.Impersonation = ImpersonationLevel.Impersonate;
            wmi = new WMIFuncs();
        }

        public override void RunCommand(string input)
        {
            CLI_Prompt newPrompt = new CLI_Prompt();
            newPrompt.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            newPrompt.Owner = App.Current.MainWindow;
            newPrompt.ShowDialog();

            try
            {
                if (!newPrompt.WasCanceled)
                {
                    ResultConsole.AddConsoleLine(newPrompt.TextBoxContents);
                    newPrompt = null;
                }
                else if (newPrompt.WasCanceled)
                {
                    newPrompt = null;
                    ResultConsole.AddConsoleLine("Action was canceled.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error running this command. \n " + ex.Message);
                ResultConsole.AddConsoleLine("Command failed with exception error caught: \n" + ex.Message);
            }
        }
    }
}
