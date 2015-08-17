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

        public override void RunCommand(string a)
        {
            List<string> devlist = ParseDeviceList(a);
            List<string> successList = GetPingableDevices.GetDevices(devlist);
            _creds = CredentialManager.Instance.UserCredentials;

            if (!ValidateCredentials(_creds))
            {
                ResultConsole.AddConsoleLine("You must enter your username and password for this command to work.");
                ResultConsole.AddConsoleLine("Run Remote Command was canceled due to improper credentials.");
                Logger.Log("Invalid credentials entered.");
                return;
            }

            foreach (var d in successList)
            {
                RunPSExecCommand.RunOnDeviceWithAuthentication(d, "cmd.exe /C gpupdate.exe /force", _creds);
                ProgressData.OnUpdateProgressBar(1);
            }
        }
    }
}