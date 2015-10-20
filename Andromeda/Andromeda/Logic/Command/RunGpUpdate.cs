using System.Collections.Generic;
using Andromeda.Model;

namespace Andromeda.Logic.Command
{
    public class RunGpUpdate : Action
    {
        private CredToken _creds;

        public RunGpUpdate()
        {
            ActionName = "Force Group Policy Update";
            Description = "Forces a GPUpdate on the machine(s). [Requires Credentials]";
            Category = ActionGroup.Other;
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
                ResultConsole.AddConsoleLine("Run Remote Command was canceled due to improper credentials.");
                Logger.Log("Invalid credentials entered.");
                return;
            }

            foreach (var device in confirmedConnectionList)
            {
                RunPSExecCommand.RunOnDeviceWithAuthentication(device, "cmd.exe /C gpupdate.exe /force", _creds);
                ProgressData.OnUpdateProgressBar(1);
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}