using System;
using System.Management;
using System.Text;
using Andromeda.Infrastructure;

namespace Andromeda
{
    public static class WMIFuncs
    {
        public static readonly string RootNamespace = "\\root\\cimv2";

        public static ManagementScope ConnectToRemoteWMI(string hostname, string scope, ConnectionOptions options)
        {
            try
            {
                ManagementScope wmiscope = new ManagementScope("\\\\" + hostname + scope, options);
                wmiscope.Connect();
                return wmiscope;
            }
            catch (Exception e)
            {
                ResultConsole.Instance.AddConsoleLine("Failed to connect to WMI namespace" + "\\\\" + hostname + scope);
                //ResultConsole.Instance.AddConsoleLine("Exception message: " + e.Message + "Inner exception: " + e.InnerException); // removed inner exception information, left for later reference
                ResultConsole.Instance.AddConsoleLine("Exception message: " + e.Message);
                Logger.Log("Error connecting to WMI namespace \\\\" + hostname + scope +
                    "\n Exception was caught: " + e.InnerException +
                    "\n Calling method: " + e.TargetSite);

                if (ConfigManager.CurrentConfig.AutomaticallyFixWmi)
                {
                    RepairRemoteWmi(hostname);
                }

                return null;
            }
            
        }

        public static ManagementScope ConnectToSCCMscope(string hostname, ConnectionOptions options)
        {
            ManagementScope ccmscope = new ManagementScope("\\\\" + hostname + "root\\ccm", options);
            ccmscope.Connect();
            return ccmscope;
        }

        public static string GetProcessReturnValueText(int retval)
        {
            switch (retval)
            {
                case 0:
                    return "0 – Success.";
                case 2:
                    return "2 – Access Denied.";
                case 3:
                    return "3 – Insufficient privilege.";
                case 8:
                    return "8 – Unknown failure.";
                case 9:
                    return "9 – Path not found";
                case 21:
                    return "21 – Invalid parameter.";
                case 1115:
                    return "1115 - A system shutdown is in progress.";
                case 1603:
                    return "1603 - ERROR_INSTALL_FAILURE: Fatal error during installation.";
            }

            return retval.ToString() + " – This return value is unknown.";
        }

        public static bool RepairRemoteWmi(string hostname)
        {
            Logger.Log("Attempting to repair WMI on device " + hostname);
            ResultConsole.Instance.AddConsoleLine("Attempting to repair WMI on device " + hostname);

            string remoteBatchPath = "\\\\" + hostname + "\\C$\\windows\\temp\\fixwmi.bat";
            string commandline = @"-i cmd /c %windir%\temp\fixwmi.bat"; // -i flag is required fore PSExec to push the command through successfully.
            var batchFileContent = CreateWmiRepairBatchConent();

            try
            {
                Logger.Log("Creating remote batch file");
                WriteToTextFile.CreateRemoteTextFile(remoteBatchPath, batchFileContent);
                
            }
            catch (Exception ex)
            {
                Logger.Log("Error creating remote WMI repair batch file. Exception thrown: " + ex.Message);
                ResultConsole.Instance.AddConsoleLine("Error creating remote WMI repair batch file. Exception thrown: " + ex.Message);
                return false;
            }

            try
            {
                Logger.Log("Run WMI repair batch on remote device " + hostname);
                RunPSExecCommand.RunOnDeviceWithAuthentication(hostname, commandline, Program.CredentialManager.UserCredentials);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log("Error running remote batch file on device " + hostname + "\n Exception: " + ex.Message);
                ResultConsole.Instance.AddConsoleLine("Error running remote batch file on device " + hostname + "\n Exception: " + ex.Message);
                return false;
            }
        }

        private static string CreateWmiRepairBatchConent()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(@"net stop winmgmt /y");
            sb.AppendLine(@"net stop wmiapsrv /y");
            sb.AppendLine(@"");
            sb.AppendLine(@"PUSHD ""%windir%\system32\wbem""");
            sb.AppendLine(@"for %%i in (*.dll) do regsvr32 -s %%i");
            sb.AppendLine(@"mofcomp.exe /RegServer");
            sb.AppendLine(@"scrcons.exe /RegServer");
            sb.AppendLine(@"unsecapp.exe /RegServer");
            sb.AppendLine(@"winmgmt.exe /RegServer");
            sb.AppendLine(@"wmiadap.exe /RegServer");
            sb.AppendLine(@"wmiapsrv.exe /RegServer");
            sb.AppendLine(@"wmiprvse.exe /RegServer");
            sb.AppendLine(@"");
            sb.AppendLine(@"PUSHD ""%windir%\SysWOW64\wbem""");
            sb.AppendLine(@"");
            sb.AppendLine(@"for %%i in (*.dll) do regsvr32 -s %%i");
            sb.AppendLine(@"mofcomp.exe /RegServer");
            sb.AppendLine(@"scrcons.exe /RegServer");
            sb.AppendLine(@"unsecapp.exe /RegServer");
            sb.AppendLine(@"winmgmt.exe /RegServer");
            sb.AppendLine(@"wmiadap.exe /RegServer");
            sb.AppendLine(@"wmiapsrv.exe /RegServer");
            sb.AppendLine(@"wmiprvse.exe /RegServer");
            sb.AppendLine(@"");
            sb.AppendLine(@"POPD");
            sb.AppendLine(@"");
            sb.AppendLine(@"""%windir%\system32\wbem\winmgmt.exe"" /resetrepository");
            sb.AppendLine(@"");
            sb.AppendLine(@"net start ccmexec");
            sb.AppendLine(@"net start winmgmt");
            sb.AppendLine(@"net start wmiapsrv");
            sb.AppendLine(@"gpupdate /force");
            sb.AppendLine(@"POPD");
            sb.AppendLine(@"Shutdown -f -r -t 0");
            sb.AppendLine(@"DEL /F /Q ""%windir%\temp\fixwmi.bat""");
            sb.AppendLine(@"EXIT");

            return sb.ToString();
        }
    }
}