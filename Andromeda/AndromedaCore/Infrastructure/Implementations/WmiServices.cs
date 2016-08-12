using System;
using System.Management;
using System.Text;
using AndromedaCore.Managers;

namespace AndromedaCore.Infrastructure
{
    public class WmiServices : IWmiServices
    {
        public string RootNamespace => "\\root\\cimv2";
        private readonly IFileAndFolderServices _fileAndFolderServices;
        private readonly IPsExecServices _psExecServices;
        private readonly ILoggerService _logger;

        public WmiServices(ILoggerService logger, IFileAndFolderServices fileAndFolderServices, IPsExecServices psExecServices)
        {
            _logger = logger;
            _fileAndFolderServices = fileAndFolderServices;
            _psExecServices = psExecServices;
        }

        public ManagementScope ConnectToRemoteWmi(string hostname, string scope, ConnectionOptions options)
        {
            try
            {
                var wmiscope = new ManagementScope($"\\\\{hostname}{scope}", options);
                wmiscope.Connect();
                return wmiscope;
            }
            catch (Exception e)
            {
                ResultConsole.Instance.AddConsoleLine($"Failed to connect to WMI namespace \\\\{hostname}{scope}");
                ResultConsole.Instance.AddConsoleLine($"Exception message: {e.Message}");
                _logger.LogError($"Error connecting to WMI namespace \\\\{hostname}{scope}", e);

                return null;
            }
            
        }

        public string GetProcessReturnValueText(int retval)
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
                default:
                    return retval + " – This return value is unknown.";
            }
        }

        public bool RepairRemoteWmi(string hostname)
        {
            _logger.LogMessage($"Attempting to repair WMI on device {hostname}");
            ResultConsole.Instance.AddConsoleLine($"Attempting to repair WMI on device {hostname}");

            var remoteBatchPath = $"\\\\{hostname}\\C$\\windows\\temp\\fixwmi.bat";
            var commandline = @"-i cmd /c %windir%\temp\fixwmi.bat"; // -i flag is required for PSExec to push the command through successfully.
            var batchFileContent = CreateWmiRepairBatchConent();

            try
            {
                _logger.LogMessage("Creating remote batch file");
                _fileAndFolderServices.CreateRemoteTextFile(remoteBatchPath, batchFileContent, _logger);
                
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating remote WMI repair batch file.", ex);
                ResultConsole.Instance.AddConsoleLine($"Error creating remote WMI repair batch file. Exception thrown: {ex.Message}");
                return false;
            }

            try
            {
                _logger.LogMessage($"Run WMI repair batch on remote device {hostname}");
                _psExecServices.RunOnDeviceWithAuthentication(hostname, commandline, CredentialManager.Instance.UserCredentials);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error running remote batch file on device {hostname}", ex);
                ResultConsole.Instance.AddConsoleLine($"Error running remote batch file on device {hostname}\n Exception: {ex.Message}");
                return false;
            }
        }

        public bool KillRemoteProcessByName(string device, string procName, ManagementScope remote)
        {
            if (remote == null)
            {
                _logger.LogWarning("Cannot kill process on null management scope.", null);
                ResultConsole.Instance.AddConsoleLine($"Unable to kill process {procName} on remote host {device}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(device) ||string.IsNullOrWhiteSpace(procName))
            {
                _logger.LogWarning("Device or process name arguments are invalid. Cannot attempt to kill process.", null);
                ResultConsole.Instance.AddConsoleLine($"Unable to kill process {procName} on remote host {device}");
                return false;
            }

            try
            {
                var procquery1 = new SelectQuery($"select * from Win32_process where name='{procName}'");

                using (var searcher = new ManagementObjectSearcher(remote, procquery1))
                {
                    var result = searcher.Get();
                    foreach (ManagementObject process in result)
                    {
                        process.InvokeMethod("Terminate", null);
                        ResultConsole.Instance.AddConsoleLine($"Called process terminate ({process["Name"]}) on device {device}.");
                        _logger.LogMessage($"Called process terminate ({process["Name"]}) on device {device}.");
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError($"There was an error trying to kill process {procName}.", e);
                ResultConsole.Instance.AddConsoleLine($"Unable to kill process {procName} on remote host {device} Error: {e.Message}");
                return false;
            }
        }

        public bool PerformRemoteUninstallByName(string device, string prodName, ManagementScope remote)
        {
            if (remote == null)
            {
                _logger.LogWarning("Cannot call product uninstall on null management scope.", null);
                ResultConsole.Instance.AddConsoleLine($"Unable to call product uninstall {prodName} on remote host {device}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(device) || string.IsNullOrWhiteSpace(prodName))
            {
                _logger.LogWarning("Device or product name arguments are invalid. Cannot attempt to call product uninstall.", null);
                ResultConsole.Instance.AddConsoleLine($"Unable to call product uninstall {prodName} on remote host {device}");
                return false;
            }

            try
            {
                var productquery = new SelectQuery("select * from Win32_product where name='" + prodName + "'");

                using (var searcher = new ManagementObjectSearcher(remote, productquery))
                {
                    foreach (ManagementObject product in searcher.Get()) // this is the fixed line
                    {
                        _logger.LogMessage($"Calling uninstall on device {device}.");
                        product.InvokeMethod("uninstall", null);
                        ResultConsole.Instance.AddConsoleLine($"Called uninstall on device {device}.");
                        _logger.LogMessage($"Called uninstall on device {device}.");
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError($"There was an error trying to call product uninstall {prodName}.", e);
                ResultConsole.Instance.AddConsoleLine($"Unable to call product uninstall {prodName} on remote host {device} Error: {e.Message}");
                return false;
            }
        }

        public bool PerformRemoteUninstallByProductId(string device, string prodId, ManagementScope remote)
        {
            if (remote == null)
            {
                _logger.LogWarning("Cannot call product uninstall on null management scope.", null);
                ResultConsole.Instance.AddConsoleLine($"Unable to call product uninstall {prodId} on remote host {device}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(device) ||
                string.IsNullOrWhiteSpace(prodId))
            {
                _logger.LogWarning("Device or product name arguments are invalid. Cannot attempt to call product uninstall.", null);
                ResultConsole.Instance.AddConsoleLine($"Unable to call product uninstall {prodId} on remote host {device}");
                return false;
            }

            try
            {
                var productquery = new SelectQuery("select * from Win32_product where identifyingnumber='" + prodId + "'");

                using (var searcher = new ManagementObjectSearcher(remote, productquery))
                {
                    foreach (ManagementObject product in searcher.Get()) // this is the fixed line
                    {
                        _logger.LogMessage($"Calling uninstall on device {device}.");
                        product.InvokeMethod("uninstall", null);
                        ResultConsole.Instance.AddConsoleLine($"Called uninstall on device {device}.");
                        _logger.LogMessage($"Called uninstall on device {device}.");
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError($"There was an error trying to call product uninstall {prodId}.", e);
                ResultConsole.Instance.AddConsoleLine($"Unable to call product uninstall {prodId} on remote host {device} Error: {e.Message}" + e.Message);
                return false;
            }
        }

        public void ForceRebootRemoteDevice(string device, ManagementScope remote)
        {
            //' Flag values:
            //' 0 - Log off
            //' 4 - Forced log off
            //' 1 - Shut down
            //' 5 - Forced shut down
            //' 2 - Reboot
            //' 6 - Forced reboot
            //' 8 - Power off
            //' 12 - Forced power off 

            ObjectQuery rebootQuery = new SelectQuery("Win32_OperatingSystem");

            using (var searcher = new ManagementObjectSearcher(remote, rebootQuery))
            {
                foreach (ManagementObject ro in searcher.Get()) // this is the fixed line
                {
                    ManagementBaseObject inParams = ro.GetMethodParameters("Win32Shutdown");

                    // Add the input parameters.
                    inParams["Flags"] = 6;

                    // Execute the method and obtain the return values.
                    ManagementBaseObject outParams = ro.InvokeMethod("Win32Shutdown", inParams, null);

                    ResultConsole.Instance.AddConsoleLine($"Reboot returned with value {GetProcessReturnValueText(Convert.ToInt32(outParams["ReturnValue"]))}");
                }
            }
        }

        private static string CreateWmiRepairBatchConent()
        {
            var sb = new StringBuilder();

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