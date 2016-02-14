using System.Collections.Generic;
using System.Linq;
using System.Management;
using Andromeda.Infrastructure;
using Andromeda.Model;

namespace Andromeda.Logic.Command
{
    public class TightVNCInstall : Action
    {
        private ConnectionOptions _connOps;
        private CredToken _creds;

        public TightVNCInstall()
        {
            ActionName = "Install TightVNC";
            Description = "Installs the version of TightVNC from the components directory.[Requires Credentials]";
            Category = ActionGroup.Other;
            _connOps = new ConnectionOptions();
        }

        public override void RunCommand(string rawDeviceList)
        {
            List<string> devlist = ParseDeviceList(rawDeviceList);
            List<string> confirmedConnectionList = GetPingableDevices.GetDevices(devlist);
            List<string> failedlist = new List<string>();

            _creds = Program.CredentialManager.UserCredentials;

            if (!Program.CredentialManager.CredentialsAreValid)
            {
                ResultConsole.AddConsoleLine("You must login for this command to work.");
                ResultConsole.AddConsoleLine("Install TightVNC Command was canceled due to improper credentials.");
                Logger.Log("Invalid credentials. Action: " + ActionName + " canceled.");
                return;
            }

            string cmdToRun = "MsiExec.exe /i " + Config.ComponentDirectory + "\\tightvnc-setup-64bit.msi" + @" /quiet /norestart ADDLOCAL=Server SET_USEVNCAUTHENTICATION=1 VALUE_OF_USEVNCAUTHENTICATION=1 SET_PASSWORD=1 VALUE_OF_PASSWORD=PASS SET_REMOVEWALLPAPER=0";
            Logger.Log("Running TightVNC Install with following command line parameters: " + cmdToRun);

            foreach (var device in confirmedConnectionList)
            {
                RunPSExecCommand.RunOnDeviceWithAuthentication(device, cmdToRun, _creds);
            }

            if (failedlist.Count > 0)
            {
                Logger.WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}