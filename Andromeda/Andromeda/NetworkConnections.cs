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

        


    }
}
