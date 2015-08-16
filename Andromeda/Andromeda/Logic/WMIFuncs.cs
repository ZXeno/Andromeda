using System;
using System.Management;
using Andromeda.Model;
using Andromeda.ViewModel;

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
                ResultConsole.Instance.AddConsoleLine("Exception: " + e.InnerException);
                Logger.Log("Error connecting to WMI namespace \\\\" + hostname + scope +
                    "\n Exception was caught: " + e.InnerException +
                    "\n Calling method: " + e.TargetSite);

                return null;
            }
            
        }

        public static bool CheckWMIAccessible(string hostname, string scope, ConnectionOptions options)
        {
            ManagementScope wmiscope = ConnectToRemoteWMI(hostname, scope, options);

            if (wmiscope.IsConnected)
            {
                ResultConsole.Instance.AddConsoleLine("Connected to WMI scope " + wmiscope.Path);
            }
            else
            {
                ResultConsole.Instance.AddConsoleLine("Connection to WMI scope " + wmiscope.Path + " failed.");
            }

            return wmiscope.IsConnected;
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
            }

            return retval.ToString() + " – This return value is unknown.";
        }


        public static void RunCommand(string device, string processToRun, CredToken creds) 
        {
            string scope = "\\root\\cimv2";
            var connOps = new ConnectionOptions();


            connOps.Username = creds.User;
            connOps.SecurePassword = creds.SecurePassword;
            connOps.Impersonation = ImpersonationLevel.Impersonate;
         
            ManagementScope deviceWMI = new ManagementScope();
            string result = "";
            
            try
            {
                if (CheckWMIAccessible(device, scope, connOps))
                {
                    deviceWMI = ConnectToRemoteWMI(device, scope, connOps);
                    ManagementPath p = new ManagementPath("Win32_Process");
                    ManagementClass wmiProcess = new ManagementClass(deviceWMI, p, null);
                    ManagementClass startupSettings = new ManagementClass("Win32_ProcessStartup");

                    startupSettings.Scope = deviceWMI;
                    startupSettings["CreateFlags"] = 0x01000000; // 0x01000000 is CREATE_BREAKAWAY_FROM_JOB creation flag, or "not a child process"

                    ManagementBaseObject inParams = wmiProcess.GetMethodParameters("Create");
                    inParams["CommandLine"] = processToRun;
                    inParams["ProcessStartupInformation"] = startupSettings;
                    ManagementBaseObject outValue = wmiProcess.InvokeMethod("Create", inParams, null);

                    if (outValue != null)
                    {
                        result = device + " returned exit code: " + GetProcessReturnValueText(Convert.ToInt32(outValue["ReturnValue"]));
                    }
                    else
                    {
                        result = "outValue was null.";
                    }
                }
                else
                {
                    result = "Unable to connect to device " + device + ".";
                }
            }
            catch (Exception ex)
            {
                result = "WMIC RUN ON DEVICE " + device + " FAILED WITH EXCEPTION \n" + ex.Message; 
            }

            Logger.Log(result);
            ResultConsole.Instance.AddConsoleLine(result);
        }
    }
}