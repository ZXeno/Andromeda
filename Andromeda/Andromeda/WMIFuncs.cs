using System;
using System.Collections.Generic;
using System.Text;
using System.Management;

namespace Andromeda
{
    public class WMIFuncs
    {


        public WMIFuncs()
        {

        }

        public ManagementScope ConnectToRemoteWMI(string hostname, string scope, ConnectionOptions options)
        {
            ManagementScope wmiscope = new ManagementScope("\\\\" + hostname + scope, options);
            wmiscope.Connect();
            return wmiscope;
        }

        public bool CheckWMIAccessible(string hostname, string scope, ConnectionOptions options)
        {
            ManagementScope wmiscope = ConnectToRemoteWMI(hostname, scope, options);
            if (wmiscope.IsConnected) { ResultConsole.AddConsoleLine("Connected to WMI scope " + wmiscope.Path); }
            else { ResultConsole.AddConsoleLine("Connection to WMI scope " + wmiscope.Path + " failed."); }
            return wmiscope.IsConnected;
        }

        public ManagementScope ConnectToSCCMscope(string hostname)
        {
            try
            {
                ManagementScope ccmscope = new ManagementScope("\\\\" + hostname + "root\\ccm");
                ccmscope.Connect();
                return ccmscope;
            }
            catch (ManagementException e)
            {
                ResultConsole.AddConsoleLine("Failed to connect.");
                ResultConsole.AddConsoleLine(e.Message);
                throw;
            }
        }

        public string GetProcessReturnValueText(int retval)
        {
            switch (retval)
            {
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
    }
}