using System.Collections.Generic;
using System.Management;
using Andromeda.Model;

namespace Andromeda.Command
{
    public class InstallTightVNC : Action
    {
        private ConnectionOptions _connOps;
        private CredToken _creds;

        public InstallTightVNC()
        {
            ActionName = "Install TightVNC";
            Description = "Installs the version of TightVNC from the components directory.[Requires Credentials]";
            Category = ActionGroup.Other;
            _connOps = new ConnectionOptions();
        }

        public override void RunCommand(string deviceList)
        {
            List<string> devlist = ParseDeviceList(deviceList);
            List<string> successList = GetPingableDevices.GetDevices(devlist);
            _creds = Program.CredentialManager.UserCredentials;

            if (!ValidateCredentials(_creds))
            {
                ResultConsole.AddConsoleLine("You must enter your USERNAME and PASSWORD above for this command to work.");
                ResultConsole.AddConsoleLine("Install TightVNC Command was canceled due to improper credentials.");
                Logger.Log("Invalid credentials entered. Action canceled.");
                return;
            }

            string cmdToRun = "MsiExec.exe /i " + Program.Config.ComponentDirectory + "\\tightvnc-setup-64bit.msi" + @" /quiet /norestart ADDLOCAL=Server SET_USEVNCAUTHENTICATION=1 VALUE_OF_USEVNCAUTHENTICATION=1 SET_PASSWORD=1 VALUE_OF_PASSWORD=PASS";
            Logger.Log("Running TightVNC Install with following command line parameters: " + cmdToRun);

            foreach (var d in successList)
            {
                RunPSExecCommand.RunOnDeviceWithAuthentication(d, cmdToRun, _creds);
            }
        }
    }
}