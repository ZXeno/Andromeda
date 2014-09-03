using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Text;

namespace Andromeda
{
    public class NetworkConnection
    {
        Ping pingSender;
        PingOptions pingOptions;

        public NetworkConnection()
        {
            pingSender = new Ping();
            pingOptions = new PingOptions();
            pingOptions.DontFragment = true;
        }

        public void PingTest(string hostname)
        {

        }

        public void PingTest(List<string> hostnames)
        {

        }

        public void PingTest(string[] hostnamearray)
        {

        }
    }
}
