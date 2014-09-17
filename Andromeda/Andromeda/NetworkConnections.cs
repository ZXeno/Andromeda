using System;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Text;

namespace Andromeda
{
    public class NetworkConnections
    {
        private Ping ping;
        private PingOptions pingOptions;

        // Constructor
        public NetworkConnections()
        {
            ping = new Ping();
            
            pingOptions = new PingOptions();
            pingOptions.DontFragment = true;
            
        }

        // Ping test for single machine.
        public PingReply PingTest(string hostname)
        {
            PingReply pr = ping.Send(hostname);
            return pr;
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


    }
}
